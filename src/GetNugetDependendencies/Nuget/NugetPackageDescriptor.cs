using System.Xml.Serialization;

namespace GetNugetDependendencies.NuGet
{
    [XmlType(AnonymousType = true)]
    public partial class NugetPackageDescriptor
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("targetFramework")]
        public string TargetFramework { get; set; }

        public override string ToString()
        {
            return string.Format("Id:{0}, Version:{1}, TargetFramework:{2}", Id, Version, TargetFramework);
        }
    }
}
