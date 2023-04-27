using BastardsMod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BastardsMod
{
    public class Line
    {
        [XmlAttribute]
        public string type; //player_line or  npc_line
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string input;
        [XmlAttribute]
        public string output;
        [XmlAttribute]
        public string text;
        [XmlAttribute]
        public int priority;

        public condition conditionD;
        public consequence consequenceD;
        public clickableCondition cConditionD;
        public persuasion pOptionD;
        public variations[] Variations;
    }
}
