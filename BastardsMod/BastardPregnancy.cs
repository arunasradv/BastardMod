using BastardsMod.Helpers;
using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BastardsMod
{
    internal class BastardPregnancy : CampaignBehaviorBase
    {
        private BastardModCfg _cfg = new BastardModCfg();

        private List<Pregnancy> _heroPregnancies = new List<Pregnancy>();
        
        public override void RegisterEvents()
        {
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, new Action<Hero, Hero,
                KillCharacterAction.KillCharacterActionDetail, bool>(this.OnHeroKilled));
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(this.DailyTickHero));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoadedEvent));
        }

        private void OnGameLoadedEvent(CampaignGameStarter obj)
        {            
            if (File.Exists(_cfg.values.file_name))
            {
                _cfg.Load(_cfg.values.file_name,typeof(BastardModConfiguration));
            }
            else
            {
                _cfg.Save(_cfg.values.file_name, _cfg.values);
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            if (victim.IsFemale)
            {
                if (_heroPregnancies.Find((Pregnancy x) => x.Mother == victim) != null)
                {
                    _heroPregnancies.RemoveAll((Pregnancy pregnancy) => pregnancy.Mother == victim);
                }
            }
        }

        private void DailyTickHero(Hero hero)
        {
            if (!CampaignOptions.IsLifeDeathCycleDisabled &&
                hero.IsFemale &&
                hero.IsAlive &&
                hero.Clan != null &&
                hero.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge &&
                hero.CurrentSettlement != null)
            {
                if (hero == Hero.MainHero && !_cfg.values.BastardPregnancyEnabledForPlayerHero)
                {
                    return;
                }

                Pregnancy pregnancy = _heroPregnancies.Find((Pregnancy x) => x.Mother == hero);
                if (hero.IsPregnant || pregnancy != null)
                {
                    if (pregnancy != null)
                    {
                        hero.IsPregnant = true;
//#warning because IsPregnant flag is cleared if normal spause pregnancy is not present, we reset this flag, because bastard pregnancy is present
                        this.CheckOffspringsToDeliver(pregnancy);
                    }
                }
                else
                {
                    this.RefreshLoverVisit(hero);
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<List<BastardPregnancy.Pregnancy>>("_heroPregnancies", ref _heroPregnancies);
        }

        public class BastardPregnancyCampaignBehaviorTypeDefiner : SaveableCampaignBehaviorTypeDefiner
        {
            public BastardPregnancyCampaignBehaviorTypeDefiner() : base(2023021800)
            {
            }
            protected override void DefineClassTypes()
            {
                base.AddClassDefinition(typeof(Pregnancy), 1, null);
            }

            protected override void DefineContainerDefinitions()
            {
                base.ConstructContainerDefinition(typeof(List<Pregnancy>));
            }
        }

        private void CheckOffspringsToDeliver(Pregnancy pregnancy)
        {
            if (!pregnancy.DueDate.IsFuture && pregnancy.Mother.IsAlive)
            {
                Hero mother = pregnancy.Mother;
                bool flag = MBRandom.RandomFloat <= _cfg.values.DeliveringTwinsProbability;
                List<Hero> list = new List<Hero>();
                int num = flag ? 2 : 1;
                int num2 = 0;
                for (int i = 0; i < num; i++)
                {
                    if (MBRandom.RandomFloat > _cfg.values.StillbirthProbability)
                    {

                        bool isOffspringFemale = MBRandom.RandomFloat <= _cfg.values.DeliveringFemaleOffspringProbability;
                        Hero item = HeroCreator.DeliverOffSpring(mother, pregnancy.Father, isOffspringFemale);
                        item.SetName(new TextObject($"{item.Name} the bastard", null), item.FirstName);
                        item.Clan = mother.Clan;
                        list.Add(item);
                    }
                    else
                    {
                        TextObject textObject = new TextObject("{=pw4cUPEn}{MOTHER.LINK} has delivered stillborn.", null);
                        StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, textObject, false);
                        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
                        num2++;
                    }
                }
                CampaignEventDispatcher.Instance.OnGivenBirth(mother, list, num2);
                _heroPregnancies.Remove(pregnancy);
                mother.IsPregnant = false;

                if (CampaignOptions.IsLifeDeathCycleDisabled && mother == Hero.MainHero)
                {
                    return;
                }

                if (MBRandom.RandomFloat <= _cfg.values.MaternalMortalityProbabilityInLabor)
                {
                    KillCharacterAction.ApplyInLabor(mother, true);
                }
            }
        }

        public void RefreshLoverVisit(Hero mother)
        {
            List<Hero> availableLovers = GetAvailableLovers(mother);

            for (int i = 0; i < availableLovers.Count; i++)
            {
                Hero father = availableLovers[i];
                if (CanMakePregnantWithBastard(mother, father))
                {
                    float chance = MBRandom.RandomFloat;
                    if (chance <= GetChanceOfPregnancyWithBastardForHero(mother, father))
                    {

                        MakePregnantWithBastard(mother, father);
                        break;
                    }
                }
            }
        }

        private List<Hero> GetAvailableLovers(Hero hero)
        {
            List<Hero> availableLovers = GetNearByMaleHeroList(hero);

            return availableLovers;
        }

        private List<Hero> GetNearByMaleHeroList(Hero hero)
        {
            List<Hero> NearByHeroes = new List<Hero>();

            Settlement heroSettlement = hero.CurrentSettlement;

            if (heroSettlement != null)
            {
                foreach (Hero h in heroSettlement.HeroesWithoutParty)
                {
                    if (!h.IsFemale &&
                        h.IsLord &&
                        h.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge &&
                        h.GetRelation(hero) > _cfg.values.BastardPregnancyHeroRelationMin)
                    {
                        if (h == Hero.MainHero && !_cfg.values.BastardPregnancyEnabledForPlayerHero)
                        {
                            continue;
                        }
                        NearByHeroes.Add(h);
                    }
                }

                foreach (MobileParty mp in heroSettlement.Parties)
                {
                    if (mp.LeaderHero != null)
                    {
                        if (!mp.LeaderHero.IsFemale &&
                            mp.LeaderHero.IsLord &&
                            mp.LeaderHero.Age > (float)Campaign.Current.Models.AgeModel.HeroComesOfAge &&
                            mp.LeaderHero.GetRelation(hero) > _cfg.values.BastardPregnancyHeroRelationMin)
                        {
                            if (mp.LeaderHero == Hero.MainHero && !_cfg.values.BastardPregnancyEnabledForPlayerHero)
                            {
                                continue;
                            }
                            NearByHeroes.Add(mp.LeaderHero);
                        }
                    }
                }
            }

            return NearByHeroes;
        }

        private float GetChanceOfPregnancyWithBastardForHero(Hero mother, Hero father)
        {
#warning Add hero perks and skils to calculation!
            CharacterTraits mt = mother.GetHeroTraits();
            CharacterTraits ft = father.GetHeroTraits();

            float chance = (
                mother.Spouse == father ||     //spouse (do not dublicate with default pregnancy mode)                                         
                                               //incest here:
                (DoesHeroResistPregnancyIncest(mother, father) &&
                (mother.Father == father ||  //father
                mother == father.Mother ||     //mother son
                mother.Father == father.Father || //sister brother
                mother.Mother == father.Mother)) ||   //sister brother
                DoesHeroResistPregnancyWithBastard(mother, father)
                ) ? -1.0f : _cfg.values.BastardPregnancyBaseChance * (((mt.Honor + ft.Honor) * (mt.Honor + ft.Honor) + (mt.Valor + ft.Valor) * (mt.Valor + ft.Valor)) / ((mt.Calculating + ft.Calculating) * (mt.Calculating + ft.Calculating) * mother.Children.Count + 1.0f));

            return chance;
        }

        private bool DoesHeroResistPregnancyIncest(Hero mother, Hero father)
        {
            bool resist = false;

            CharacterTraits mt = mother.GetHeroTraits();
            CharacterTraits ft = father.GetHeroTraits();

            resist = mt.Honor + ft.Honor >= _cfg.values.ResistPregnancyIncestHonor ||
                mt.Valor + ft.Valor <= _cfg.values.ResistPregnancyIncestValor ||
                mt.Calculating + ft.Calculating >= _cfg.values.ResistPregnancyIncestCalculating;

            return resist;
        }

        private bool DoesHeroResistPregnancyWithBastard(Hero mother, Hero father)
        {
            bool resist = false;

            CharacterTraits mt = mother.GetHeroTraits();
            CharacterTraits ft = father.GetHeroTraits();

            resist = mt.Honor + ft.Honor >= _cfg.values.ResistPregnancyWithBastardHonor ||
                mt.Valor + ft.Valor <= _cfg.values.ResistPregnancyWithBastardValor ||
                mt.Calculating + ft.Calculating >= _cfg.values.ResistPregnancyWithBastardCalculating;

            return resist;
        }

        public bool CanMakePregnantWithBastard(Hero mother, Hero father)
        {
            Pregnancy pregnancy = this._heroPregnancies.Find((Pregnancy x) => x.Mother == mother);
            if (mother.IsPregnant || pregnancy != null || father == mother.Spouse || mother.Children.Count >= _cfg.values.BastardPregnancyMaxChildren)
            {
                return false;
            }
            return true;
        }

        public void MakePregnantWithBastard(Hero mother, Hero father)
        {
            this._heroPregnancies.Add(new Pregnancy(mother, father, CampaignTime.DaysFromNow(_cfg.values.PregnancyDurationInDays)));
            mother.IsPregnant = true;
            LogEntry.AddLogEntry(new PregnancyLogEntry(mother));
            TextObject textObject = new TextObject("{=*}{MOTHER.LINK} got pregnant with {FATHER.LINK}'s bastard child", null);
            StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, textObject, false);
            StringHelpers.SetCharacterProperties("FATHER", father.CharacterObject, textObject, false);
            InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
            log(textObject.ToString());
        }
            private void log(string text)
            {
                string output = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PregnancyOutPut.txt");
                File.AppendAllText(output, $"{CampaignTime.Now}: {text}\r");
            }
        internal class Pregnancy
        {
            public Pregnancy(Hero pregnantHero, Hero father, CampaignTime dueDate)
            {
                this.Mother = pregnantHero;
                this.Father = father;
                this.DueDate = dueDate;
            }

            [SaveableField(1)]
            public readonly Hero Mother;

            [SaveableField(2)]
            public readonly Hero Father;

            [SaveableField(3)]
            public readonly CampaignTime DueDate;
        }
    }
}