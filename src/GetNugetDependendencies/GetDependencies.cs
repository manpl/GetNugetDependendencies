using GetNugetDependendencies.NuGet;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace GetNugetDependendencies
{
    [Cmdlet(VerbsCommon.Get, "Dependencies", DefaultParameterSetName = "Inline")]
    public class GetDependencies : PSCmdlet
    {
        [Parameter(Mandatory = true,HelpMessage = "Path to packages.config file")]
        [ValidateNotNullOrEmpty]
        public string NugetConfigPath { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Packages source. Defaults to 'https://packages.nuget.org/api/v2'")]
        [ValidateNotNullOrEmpty]
        public string NugetSource { get; set; }


        public string Framework { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (string.IsNullOrEmpty(NugetSource))
            {
                NugetSource = "https://packages.nuget.org/api/v2";
            }

            if (string.IsNullOrEmpty(Framework))
            {
                Framework = ".NETFramework";
            }

            this.WriteDebug(string.Format("BeginProcessing. NugetSrc:{0}, Fx:{1}", NugetSource, Framework));
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var repository = PackageRepositoryFactory.Default.CreateRepository(this.NugetSource);
            var config = PackagesConfig.ReadFromFile(NugetConfigPath);

            foreach (var packageDesc in config.Packages)
            {
                WriteDebug("Processing "+ packageDesc.ToString());
                var version = SemanticVersion.Parse(packageDesc.Version);
                var package = repository.FindPackage(packageDesc.Id, version, true, true);

                ListDependencies(package, repository);
            }
        }

        private void ListDependencies(IPackage package, IPackageRepository repository, string prefix = "")
        {
            WriteObject((string.IsNullOrEmpty(prefix) ? "" : " |" + prefix) + package.GetFullName());
            var highestFxVersion = package.GetSupportedFrameworks().Where(fx => fx.Identifier == this.Framework).OrderByDescending(v => v.Version).First();

            var dependencies = package.GetCompatiblePackageDependencies(highestFxVersion);

            foreach (var dep in dependencies)
            {
                var dependantPackage = repository.ResolveDependency(dep, true, true);
                ListDependencies(dependantPackage, repository, prefix + "---");
            }
        }
    }
}
