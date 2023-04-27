using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BastardsMod
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignGameStarter = gameStarterObject as CampaignGameStarter;

                if (campaignGameStarter != null)
                {
                    campaignGameStarter.AddBehavior(new BastardPregnancy());
                    campaignGameStarter.AddBehavior(new LordNeedBastardBackIssueBehavior());                    
                    campaignGameStarter.AddBehavior(new BastardMakeBahavior()); 
                    campaignGameStarter.AddBehavior(new PlayerNeedBastardBackIssueBehavior());
                }
            }
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

    }
}
