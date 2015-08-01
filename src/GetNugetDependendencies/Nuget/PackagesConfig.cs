using System.IO;
using System.Xml.Serialization;

namespace GetNugetDependendencies.NuGet
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "packages")]
    public partial class PackagesConfig
    {
        [XmlElement("package")]
        public NugetPackageDescriptor[] Packages { get; set; }

        public static PackagesConfig ReadFromFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PackagesConfig));

            using (var fileStream = File.OpenRead(path))
            {
                return (PackagesConfig)serializer.Deserialize(fileStream);
            }
        }
    }
}
