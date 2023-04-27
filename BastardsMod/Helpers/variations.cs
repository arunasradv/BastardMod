using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class variations
    {
        [XmlAttribute]
        public string text;
        public tag[] tag;
    }
}
