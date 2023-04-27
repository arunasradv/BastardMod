using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class condition
    {
        [XmlAttribute]
        public string method;
        [XmlAttribute]
        public string[] parameters;
    }
}
