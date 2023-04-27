using System.Collections.Generic;
using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class persuasionTask
    {
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string spokenLine;
        [XmlAttribute]
        public string immediateFailLine;
        [XmlAttribute]
        public string finalFailLine;
        [XmlAttribute]
        public string tryLaterLine;
        [XmlAttribute]
        public int reservationType;

        public List<persuasionOption> Options = new List<persuasionOption>();
    }
}
