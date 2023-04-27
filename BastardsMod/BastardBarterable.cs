using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BastardsMod
{
    public class BastardBarterable : Barterable
    {
        public override string StringID
        {
            get
            {
                return "bastard_barterable";
            }
        }
        
        public BastardBarterable(Hero originalOwner, PartyBase originalParty, Hero heroBeingProposedTo, Hero proposingHero) : base(originalOwner, originalParty)
        {
            this.HeroBeingProposedTo = heroBeingProposedTo;
            this.ProposingHero = proposingHero;
        }

        public override TextObject Name
        {
            get
            {
                StringHelpers.SetCharacterProperties("HERO_BEING_PROPOSED_TO", this.HeroBeingProposedTo.CharacterObject, null, false);
                StringHelpers.SetCharacterProperties("PROPOSING_HERO", this.ProposingHero.CharacterObject, null, false);
                return new TextObject("{=*}{HERO_BEING_PROPOSED_TO.NAME} join {PROPOSING_HERO.NAME}", null);
            }
        }

        public override void Apply()
        {
            this.HeroBeingProposedTo.Clan = this.ProposingHero.Clan;
        }

        public override int GetUnitValueForFaction(IFaction faction)
        {
            float value = -HeroBeingProposedTo.Age * 10000;
            return (int)value;
        }

        public override ImageIdentifier GetVisualIdentifier()
        {
            return new ImageIdentifier(CharacterCode.CreateFrom(this.HeroBeingProposedTo.CharacterObject));
        }
       
        public readonly Hero ProposingHero;

        public readonly Hero HeroBeingProposedTo;
    }
}
