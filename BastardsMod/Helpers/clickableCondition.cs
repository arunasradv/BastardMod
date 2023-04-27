using System.Xml.Serialization;

namespace BastardsMod
{
    public class clickableCondition
    {
        [XmlAttribute]
        public string method;
        [XmlAttribute]
        public string[] parameters;
    }
}
