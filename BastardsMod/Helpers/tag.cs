using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public struct tag
    {
        [XmlAttribute]
        public string tag_name;
        [XmlAttribute]
        public int weight;
    }
}
