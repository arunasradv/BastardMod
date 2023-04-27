using System.Collections.Generic;
using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class Dialog
    {
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string input;
        [XmlAttribute]
        public int priority;

        public List<persuasionTask> Tasks = new List<persuasionTask>();

        public List<Line> lines = new List<Line>();
    }
}
