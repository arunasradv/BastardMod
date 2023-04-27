using System.Xml.Serialization;

namespace BastardsMod.Helpers
{
    public class persuasionOption
    {
        [XmlAttribute]
        public string defaultSkills;
        [XmlAttribute]
        public string defaultTraits;
        [XmlAttribute]
        public string traitEffect;
        [XmlAttribute]
        public string persuasionArgumentStrength;
        [XmlAttribute]
        public bool givesCriticalSuccess;
        [XmlAttribute]
        public string line;

        public traitCorrelations traitCorrelations;

        [XmlAttribute]
        public bool canBlockOtherOption;
        [XmlAttribute]
        public bool canMoveToTheNextReservation;
        [XmlAttribute]
        public bool isInitiallyBlocked;

        public reaction[] optionReactions;
    }
}
