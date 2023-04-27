using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class consequence
    {
        [XmlAttribute]
        public string method;
        [XmlAttribute]
        public string[] parameters;
    }
}
