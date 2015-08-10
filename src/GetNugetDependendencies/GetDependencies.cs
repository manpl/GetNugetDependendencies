using GetNugetDependendencies.NuGet;
using NuGet;
using System.Linq;
using System.Management.Automation;
using System;

namespace GetNugetDependendencies
{
    [Cmdlet(VerbsCommon.Get, "Dependencies", DefaultParameterSetName = "Inline")]
    public class GetDependencies : PSCmdlet
    {
        [Parameter(Mandatory = true,HelpMessage = "Path to packages.config file", ParameterSetName = "Config")]
        [ValidateNotNullOrEmpty]
        public string NugetConfigPath { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Id of the package", ParameterSetName = "Package")]
        [ValidateNotNullOrEmpty]
        public string PackageId { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Version of the package", ParameterSetName = "Package")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Packages source. Defaults to 'https://packages.nuget.org/api/v2'")]
        [ValidateNotNullOrEmpty]
        public string NugetSource { get; set; }

        //[Parameter(Mandatory = false, HelpMessage = "Framework")]
        //public string Framework { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            if (string.IsNullOrEmpty(NugetSource))
            {
                NugetSource = "https://packages.nuget.org/api/v2";
            }

            //if (string.IsNullOrEmpty(Framework))
            //{
            //    Framework = ".NETFramework";
            //}

            this.WriteDebug(string.Format("BeginProcessing. NugetSrc:{0}", NugetSource));
            //this.WriteDebug(string.Format("BeginProcessing. NugetSrc:{0}, Fx:{1}", NugetSource, Framework));
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var repository = PackageRepositoryFactory.Default.CreateRepository(this.NugetSource);
            
            if(this.ParameterSetName == "Config")
            {
                ProcessConfig(repository);
            }
            else
            {
                ProcessPackage(repository);
            }
        }

        private void ProcessPackage(IPackageRepository repository)
        {
            WriteDebug("Processing " + this.PackageId.ToString() + (string.IsNullOrEmpty(Version) ? "" : ("@" + Version)));

            IPackage package;

            if (string.IsNullOrEmpty(Version))
            {
                package = repository.FindPackagesById(this.PackageId).OrderByDescending(p => p.Version).FirstOrDefault();
            }
            else
            {
                var version = SemanticVersion.Parse(this.Version);
                package = repository.FindPackage(this.PackageId, version, true, true);
            }


            ListDependencies(package, repository);
            WriteObject("");
        }

        private void ProcessConfig(IPackageRepository repository)
        {
            var config = PackagesConfig.ReadFromFile(NugetConfigPath);

            foreach (var packageDesc in config.Packages)
            {
                WriteDebug("Processing " + packageDesc.ToString());
                var version = SemanticVersion.Parse(packageDesc.Version);
                var package = repository.FindPackage(packageDesc.Id, version, true, true);

                ListDependencies(package, repository);
                WriteObject("");
            }
        }

        private void ListDependencies(IPackage package, IPackageRepository repository, string prefix = "")
        {
            if(package == null)
            {
                WriteDebug("Null package passed");
                return;
            }

            WriteObject(prefix + "-- " + package.GetFullName());
            //var highestFxVersion = package.GetSupportedFrameworks().OrderByDescending(v => v.Version).First();
            var dependencies = package.GetCompatiblePackageDependencies(null).ToList();

            if (!dependencies.Any())
            {
                return;
            }

            foreach (var dep in dependencies)
            {
                var dependantPackage = repository.ResolveDependency(dep, true, true);
                ListDependencies(dependantPackage, repository,  prefix + "   |");
            }
        }
    }
}
