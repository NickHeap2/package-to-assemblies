using System.Collections.Generic;
using System.Xml.Serialization;

namespace package_to_assemblies
{
    [XmlRoot("packages")]
    public class Packages
    {
        public Packages() { Items = new List<Package>(); }
        public Package this[int i]
        {
            get { return Items[i]; }
            set { Items[i] = value; }
        }

        [XmlElement("package")]
        public List<Package> Items { get; set; }
    }
}