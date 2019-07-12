using System.Xml.Serialization;

namespace package_to_assemblies
{
    [XmlType("package")]
    public class Package
    {
        [XmlAttribute("id")]
        public string id { get; set; }
        [XmlAttribute("version")]
        public string version { get; set; }
        [XmlAttribute("targetFramework")]
        public string targetFramework { get; set; }
    }
}