using GetNugetDependendencies.NuGet;
using NuGet;
using System.Linq;
using System.Management.Automation;
using System;
using GetNugetDependendencies.DataStructures;

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

        private IPackageRepository Repository;

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

            Repository = PackageRepositoryFactory.Default.CreateRepository(this.NugetSource);
            
            if(this.ParameterSetName == "Config")
            {
                ProcessConfig();
            }
            else
            {
                ProcessPackage();
            }
        }

        private void ProcessPackage()
        {
            WriteDebug("Processing " + this.PackageId.ToString() + (string.IsNullOrEmpty(Version) ? "" : ("@" + Version)));

            IPackage package;

            if (string.IsNullOrEmpty(Version))
            {
                package = Repository.FindPackagesById(this.PackageId).OrderByDescending(p => p.Version).FirstOrDefault();
            }
            else
            {
                var version = SemanticVersion.Parse(this.Version);
                package = Repository.FindPackage(this.PackageId, version, true, true);
            }

            var dependencyTree = new TreeElement<IPackage>(package);

            ListDependencies(dependencyTree, dependencyTree);
            WriteObject("");
        }

        private void ProcessConfig()
        {
            //var config = PackagesConfig.ReadFromFile(NugetConfigPath);

            //foreach (var packageDesc in config.Packages)
            //{
            //    WriteDebug("Processing " + packageDesc.ToString());
            //    var version = SemanticVersion.Parse(packageDesc.Version);
            //    var package = Repository.FindPackage(packageDesc.Id, version, true, true);

            //    ListDependencies(null, package);
            //    WriteObject("");
            //}
        }

        private void ListDependencies(TreeElement<IPackage> dependencyTree, TreeElement<IPackage> currentElement)
        {
            WriteDebug("Processing dependencies" + currentElement.Element.GetFullName());

            //if(package == null)
            //{
            //    WriteDebug("Null package passed");
            //    return;
            //}

            //WriteObject(prefix + "-- " + package.GetFullName());
            //var highestFxVersion = currentElement.Element.GetSupportedFrameworks().OrderByDescending(v => v.Version).First();
            var dependencies = currentElement.Element.GetCompatiblePackageDependencies(null);
            //var dependencies = currentElement.Element.GetCompatiblePackageDependencies(highestFxVersion).ToList();

            if (!dependencies.Any())
            {
                return;
            }

            foreach (var dep in dependencies)
            {
                var dependantPackage = Repository.ResolveDependency(dep, true, true);

                var inTheTree = dependencyTree.Get(dependantPackage);

                if(inTheTree != null)
                {
                    currentElement.AddChild(inTheTree);
                    WriteDebug("element already in the tree " + dependantPackage.GetFullName());
                    return;
                }
                
                //dependencyTree.AddChildren();
                ListDependencies(dependencyTree, currentElement.AddChild(dependantPackage));
            }

            //WriteObject(prefix);
        }
    }
}
