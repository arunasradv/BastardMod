using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class reaction
    {
        [XmlAttribute]
        public string persuasionOptionResult;
        [XmlAttribute]
        public string text;
    }
}
