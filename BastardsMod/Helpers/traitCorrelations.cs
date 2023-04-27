using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class traitCorrelations
    {
        [XmlAttribute]
        public int valor = 0;
        [XmlAttribute]
        public int mercy = 0;
        [XmlAttribute]
        public int honor = 0;
        [XmlAttribute]
        public int generosity = 0;
        [XmlAttribute]
        public int calculating = 0;
    }
}
