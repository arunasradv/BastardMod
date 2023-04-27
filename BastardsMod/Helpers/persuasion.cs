using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class persuasion
    {
        [XmlAttribute]
        public string method;
        [XmlAttribute]
        public string[] parameters;
    }
}
