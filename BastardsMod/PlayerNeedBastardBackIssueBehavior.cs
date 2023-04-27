using BastardsMod.Helpers;
using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BastardsMod
{
	/// <summary>
	/// Issue what is activated after talking to Player's bastard child owner (mother). And after requesting to return bastards
	/// will get tasks:
	/// Persuede for "care" payment
	/// Persuede for compensation ("moral...")
	/// Persuede for relation (improve relation with bastard to xxx, when age is > 18)
	/// Persuede for relation (improve relation with mother to xxx)
	/// Persuede duel/fight (in persuation failure case and mother imprisoned/killed)
	/// </summary>
	internal class PlayerNeedBastardBackIssueBehavior : CampaignBehaviorBase
	{
		public override void RegisterEvents()
		{
			CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnCheckForIssue));
			//CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, new Action<Hero>(this.DailyTickHero));
		}
#warning temporary for test
		//private void DailyTickHero(Hero obj)
  //      {
		//	Hero.MainHero.Gold += 100000;
		//}

        public override void SyncData(IDataStore dataStore)
		{

		}

		public void OnCheckForIssue(Hero hero)
		{
			if (this.SuitableCondition(hero))
			{
				Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnStartIssue),
					typeof(PlayerNeedsBastardsIssue), IssueBase.IssueFrequency.VeryCommon));
				return;
			}
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(
				typeof(PlayerNeedsBastardsIssue), IssueBase.IssueFrequency.VeryCommon));
		}

		public bool SuitableCondition(Hero hero)
		{
			return hero.Age >= 18f && hero.IsLord && hero.IsActive && hero.IsFemale && HeroHasBastardsInHero2Clan(Hero.MainHero,hero);
		}

		private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
		{
			return new PlayerNeedsBastardsIssue(issueOwner, pid);
		}

		private List<Hero> GetHeroBastardsInHero2Clan(Hero hero, Hero hero2)
		{
			List<Hero> list = hero.Children.Where((x) => x.Name.Contains("the bastard") &&
			x.Clan == hero2.Clan).ToList();
			return list;
		}
		private bool HeroHasBastardsInHero2Clan(Hero hero, Hero hero2)
		{
			List<Hero> childrens = GetHeroBastardsInHero2Clan(hero, hero2);
			if (childrens != null && childrens.Count > 0)
				return true;
			return false;
		}

		public class PlayerNeedsBastardsIssue : IssueBase
		{
			[SaveableField(10)]
			private List<Hero> _bastardHeroes;

			public PlayerNeedsBastardsIssue(Hero issueOwner, PotentialIssueData potentialIssueData) : base(issueOwner, CampaignTime.DaysFromNow(200f))
			{
				_bastardHeroes = Hero.MainHero.Children.Where((x) => x.Name.Contains("the bastard") && x.Clan == base.IssueOwner.Clan).ToList();
			}
			public override TextObject IssueBriefByIssueGiver
			{
				get
				{
					TextObject textObject = new TextObject("{=*}What problem?[ib:nervous][ib:closed]", null);
					return textObject;
				}
			}
			public override TextObject IssueAcceptByPlayer
			{
				get
				{
					return new TextObject("{=*}My bastard children one.", null);
				}
			}
			public override TextObject IssueQuestSolutionExplanationByIssueGiver
			{
				get
				{
					return new TextObject("{=*}I have no problems with it.[ib:nervous][ib:closed]", null);
				}
			}
			public override TextObject IssueQuestSolutionAcceptByPlayer
			{
				get
				{
					return new TextObject("{=*}I need my children back!", null);
				}
			}
			public override bool IsThereAlternativeSolution
			{
				get
				{
					return false;
				}
			}
			public override bool IsThereLordSolution
			{
				get
				{
					return false;
				}
			}
			public override TextObject Title
			{
				get
				{
					TextObject textObject = new TextObject("{=*}{?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} has my bastard children", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject, false);
					return textObject;
				}
			}
			public override TextObject Description
			{
				get
				{
					TextObject textObject = new TextObject("{=*}{QUEST_GIVER.NAME}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of the {CLAN}, has my bastard children. And I want bastard children back to me.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject, false);
					textObject.SetTextVariable("CLAN", base.IssueOwner.Clan.Name);
					return textObject;
				}
			}
			public override IssueFrequency GetFrequency()
			{
				return IssueBase.IssueFrequency.VeryCommon;
			}
			public override bool IssueStayAliveConditions()
			{
				return base.IssueOwner.Clan != Clan.PlayerClan && base.IssueOwner.IsAlive;
			}
			protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
			{
				relationHero = null;
				skill = null;
				flag = IssueBase.PreconditionFlags.None;
				if (issueGiver.GetRelationWithPlayer() < -80f)
				{
					flag |= IssueBase.PreconditionFlags.Relation;
					relationHero = issueGiver;
				}
				//if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
				//{
				//	flag |= IssueBase.PreconditionFlags.AtWar;
				//}
				return flag == IssueBase.PreconditionFlags.None;
			}
			protected override void CompleteIssueWithTimedOutConsequences()
			{
			}
			protected override QuestBase GenerateIssueQuest(string questId)
			{
				return new PlayerNeedsBastardsIssueQuest(questId, base.IssueOwner, CampaignTime.DaysFromNow(2000f), 0);
			}			
			protected override void OnGameLoad()
			{
			}
		}

		public class PlayerNeedsBastardsIssueQuest : QuestBase
		{
			[SaveableField(80)]
			private JournalLog _startQuestLog;
			[SaveableField(10)]
			private List<Hero> _bastardHeroes;
			[SaveableField(20)]
			private bool _checkForMissionEnd;
			[SaveableField(30)]
			private bool _canBarterBastard; 
			[SaveableField(40)]
			private bool _lordIsPersueded;
			public PlayerNeedsBastardsIssueQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base(questId, questGiver, duration, rewardGold)
			{
				_bastardHeroes = Hero.MainHero.Children.Where((x) => x.Name.Contains("the bastard") && x.Clan == base.QuestGiver.Clan).ToList();
				SetDialogs();
				base.InitializeQuestOnCreation();
			}

			public override TextObject Title
			{
				get
				{
					TextObject textObject = new TextObject("{=*}I need bastard children back from {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {?QUEST_GIVER.LINK}", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
					return textObject;
				}
			}
			public override bool IsRemainingTimeHidden
			{
				get
				{
					return false;
				}
			}

			protected override void InitializeQuestOnGameLoad()
			{
				SetDialogs();
			}
			private void QuestAcceptedConsequences()
			{
				base.StartQuest();
				TextObject PlayerStartsQuestLogText = new TextObject("{=*}{QUEST_GIVER.LINK}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of the {CLAN} clan has my bastard children bring them back in {DURATION} days.", null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, PlayerStartsQuestLogText, false);
				PlayerStartsQuestLogText.SetTextVariable("DURATION", 200);
				PlayerStartsQuestLogText.SetTextVariable("CLAN", base.QuestGiver.Clan.EncyclopediaLinkWithName);

				this._startQuestLog = base.AddLog(PlayerStartsQuestLogText);

				for (int i = 0; i < this._bastardHeroes.Count; i++)
				{
					AddHeroToQuestLog(i);
				}

				this._checkForMissionEnd = true;
			}

			private void AddHeroToQuestLog(int i)
			{
				string text = "{=*}{QUEST_BASTARD_HERO.LINK} from mother {MOTHER.LINK} of {CLAN}";
				TextObject textObject = new TextObject(text, null);
				StringHelpers.SetCharacterProperties("MOTHER", _bastardHeroes[i].Mother.CharacterObject, textObject, false);
				StringHelpers.SetCharacterProperties("QUEST_BASTARD_HERO", _bastardHeroes[i].CharacterObject, textObject, false);
				textObject.SetTextVariable("CLAN", _bastardHeroes[i].Mother.Clan.EncyclopediaLinkWithName);
				string taskText = "{=*}{NAME}";
				TextObject taskTextObject = new TextObject(taskText, null);
				StringHelpers.SetCharacterProperties("NAME", _bastardHeroes[i].CharacterObject, taskTextObject, false);
				this._startQuestLog = base.AddDiscreteLog(textObject, taskTextObject, (_bastardHeroes[i].Clan == Hero.MainHero.Clan ? 1 : 0), 1, null, false);
			}
			private bool NotableDialogCondition()
			{
				return Hero.OneToOneConversationHero == base.QuestGiver;
			}
			private void leave_encounter_on_consequence()
			{
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
			}

            #region EVENTS related to hero
            protected override void RegisterEvents()
			{
				CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.OnHourlyTick));
				CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(this.OnHeroKilled));
				CampaignEvents.WarDeclared.AddNonSerializedListener(this, new Action<IFaction, IFaction, DeclareWarAction.DeclareWarDetail>(this.OnWarDeclared));
				CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, new Action<Clan, Kingdom, Kingdom, ChangeKingdomAction.ChangeKingdomActionDetail, bool>(this.OnClanChangedKingdom));
				CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(this.OnMapEventStarted));
				CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.OnSettlementEntered));
				CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, new Action(this.OnGameLoadFinished));
				CampaignEvents.HeroCreated.AddNonSerializedListener(this, new Action<Hero, bool>(this.OnHeroCreated));
			}
			private void OnHeroCreated(Hero hero, bool isBornNaturally)
			{
				if (hero.Father == base.QuestGiver && hero.Name.Contains("the bastard"))
				{
					if (_bastardHeroes.Where((x) => x.Name.ToString() == hero.Name.ToString()).IsEmpty())
					{
						_bastardHeroes.Add(hero);
						AddHeroToQuestLog(_bastardHeroes.IndexOf(hero));
					}
				}
			}
			protected override void OnTimedOut()
			{
				TextObject textObject = new TextObject("{=taz5cAtw}You failed to return bastards from {QUEST_GIVER.LINK}.", null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
				base.AddLog(textObject, false);
				this.RelationshipChangeWithQuestGiver = -10;
			}
			private void OnGameLoadFinished()
			{
				if (Settlement.CurrentSettlement != null)
				{
					for (int i = 0; i < this._bastardHeroes.Count; i++)
					{
						if (Settlement.CurrentSettlement.HeroesWithoutParty.Contains(this._bastardHeroes[i]))
						{
							this.SpawnHeroInLordsHall(this._bastardHeroes[i]);
						}
					}
				}
			}
			private void SpawnHeroInLordsHall(Hero hero)
			{
				Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(hero.CharacterObject.Race, "_settlement");
				ValueTuple<string, Monster> valueTuple = new ValueTuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, hero.CharacterObject.IsFemale, "_lord"), monsterWithSuffix);
				IFaction mapFaction = hero.MapFaction;
				uint color = (mapFaction != null) ? mapFaction.Color : 4291609515U;
				IFaction mapFaction2 = hero.MapFaction;
				uint color2 = (mapFaction2 != null) ? mapFaction2.Color : 4291609515U;
				AgentData agentData = new AgentData(new SimpleAgentOrigin(hero.CharacterObject, -1, null, default(UniqueTroopDescriptor))).Monster(valueTuple.Item2).NoHorses(true).ClothingColor1(color).ClothingColor2(color2);
				LocationComplex.Current.GetLocationWithId("lordshall").AddCharacter(new LocationCharacter(agentData, new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
					"sp_notable", true, LocationCharacter.CharacterRelations.Neutral, valueTuple.Item1, true, false, null, false, false, true));
			}
			private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
			{
				if (party != null && party.IsMainParty)
				{
					if (Settlement.CurrentSettlement != null)
					{
						for (int i = 0; i < this._bastardHeroes.Count; i++)
						{
							if (Settlement.CurrentSettlement.HeroesWithoutParty.Contains(this._bastardHeroes[i]))
							{
								this.SpawnHeroInLordsHall(this._bastardHeroes[i]);
							}
						}
					}
				}
			}
			private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
			{
				//TextObject textObject = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.", null);
				//StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
				//QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, textObject, this.WarDeclaredQuestCancel);
			}
			private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
			{
				if (!_bastardHeroes.Where((x) => x.Name.ToString() == victim.Name.ToString()).IsEmpty())
				{
					string text = "{=*}{QUEST_BASTARD_HERO.LINK} has died!";
					TextObject textObject = new TextObject(text, null);
					StringHelpers.SetCharacterProperties("QUEST_BASTARD_HERO", victim.CharacterObject, textObject, false);
					base.AddLog(textObject, false);
					textObject = new TextObject("{=abmSQFR5}{QUEST_GIVER.LINK} is furious.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
					base.AddLog(textObject, false);
					this.RelationshipChangeWithQuestGiver = -40;
					base.CompleteQuestWithFail(null);
				}
			}
			public void OnHourlyTick()
			{
				if (base.IsOngoing && !Hero.MainHero.IsPrisoner)
				{
					if (this._checkForMissionEnd)
					{
						this._checkForMissionEnd = false;
					}
				}
			}
			public override void OnHeroCanBeSelectedInInventoryInfoIsRequested(Hero hero, ref bool result)
			{
				this.CommonRestrictionInfoIsRequested(hero, ref result);
			}
			public override void OnHeroCanHavePartyRoleOrBeGovernorInfoIsRequested(Hero hero, ref bool result)
			{
				this.CommonRestrictionInfoIsRequested(hero, ref result);
			}
			public override void OnHeroCanLeadPartyInfoIsRequested(Hero hero, ref bool result)
			{
				this.CommonRestrictionInfoIsRequested(hero, ref result);
			}
			public override void OnHeroCanHaveQuestOrIssueInfoIsRequested(Hero hero, ref bool result)
			{
				this.CommonRestrictionInfoIsRequested(hero, ref result);
			}
			public override void OnHeroCanMarryInfoIsRequested(Hero hero, ref bool result)
			{
				this.CommonRestrictionInfoIsRequested(hero, ref result);
			}
			private void CommonRestrictionInfoIsRequested(Hero hero, ref bool result)
			{
				result = _bastardHeroes.Where((x) => x.Name.ToString() == hero.Name.ToString()).IsEmpty();
			}
			private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
			{
				if (QuestHelper.CheckMinorMajorCoercion(this, mapEvent, attackerParty))
				{
					QuestHelper.ApplyGenericMinorMajorCoercionConsequences(this, mapEvent);
				}
			}
			private TextObject WarDeclaredQuestCancel
			{
				get
				{
					TextObject textObject = new TextObject("{=HkbK8cqw}Your clan is now at war with the {QUEST_GIVER.LINK}'s faction. Your agreement with {QUEST_GIVER.LINK} was terminated.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
					return textObject;
				}
			}
			private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
			{
				if (base.QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
				{
					base.CompleteQuestWithCancel(this.WarDeclaredQuestCancel);
				}
			}
            #endregion

            #region DIALOG
            private List<PersuasionTask> _allReservations = new List<PersuasionTask>();
			private DialogHelper AllDialogs = new DialogHelper();
			public void AddDefault()
			{
				AllDialogs.dialogs.Add(
				new Dialog
				{
					id = "player_need_bastards_quest_start",
					input = "issue_classic_quest_start",
					priority = 125,

					lines =
					{
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_start_0",
							input = "issue_classic_quest_start",
							output = "player_need_bastards_quest_start_1",
							priority = 125,
							text = "{=*}And why I should?[if:idle_angry][ib:closed]",
							conditionD = new condition { method = "NotableDialogCondition"},
							consequenceD = new consequence { method = "QuestAcceptedConsequences"}
						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_start_1",
							input = "player_need_bastards_quest_start_1",
							output = "player_need_bastards_persuasion_start_1",
							priority = 125,
							text = "{=*}Let me tell you something...",
							consequenceD = new consequence { method = "conversation_unblock_all_persuasion_task" },
						}
					}
				});

				AllDialogs.dialogs.Add(new Dialog
				{
					id = "player_need_bastards_persuasion_start_1",
					input = "player_need_bastards_persuasion_start_1",
					priority = 125,

					Tasks =
				{
					new persuasionTask
					{
						 id = "give_bastards_quest_persuade_task_0",
						 spokenLine = "{=*}",
						 finalFailLine = "{=*}Guards![if:convo_bared_teeth][if:idle_angry]",
						 tryLaterLine = "{=*}I'm sorry.[ib:nervous][ib:closed]",
						 Options =
						 {
							new persuasionOption
							{
								line = "{=*}Just tell me how much money do you need, let's do barter?",
								defaultSkills = "Trade",
								defaultTraits = "Honor",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = true,
								traitCorrelations = new traitCorrelations { valor = 0,  mercy = 1,  honor = -1,  generosity = 0,  calculating = -1 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] {
									new reaction { persuasionOptionResult = "Success", text = "Maybe it could work."},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "I agree, I have a price.[ib:happy]" },
									new reaction { persuasionOptionResult = "Failure", text = "No. It is not for me.[ib:nervous][ib:closed]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "How dare you? I'm not selling my children![ib:closed][if:idle_angry][ib:aggressive]" },
								}
							},
							new persuasionOption
							{
								line = "{=*}It is my turn to fulfill bastard children needs.",
								defaultSkills = "Leadership",
								defaultTraits = "Mercy",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = true,
								traitCorrelations = new traitCorrelations { valor = 0,  mercy = 1,  honor = 0,  generosity = 1,  calculating = 0 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] {
									new reaction { persuasionOptionResult = "Success", text = "Fine.[rf:convo_relaxed_happy]"},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "Great, let you have it now.[ib:happy][ib:normal]" },
									new reaction { persuasionOptionResult = "Failure", text = "No.I doubt it.[if:idle_angry]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "Never![ib:nervous][ib:closed][if:idle_angry]" },
								}
							}
						 },
						immediateFailLine = "",
						reservationType = 0
					}
					},

					lines = {
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_persuasion_start_introduction",
							input = "player_need_bastards_persuasion_start_1",
							output = "player_need_bastards_quest_persuade_start_reservation",
							priority = 0,
							text = "{=*}What is it?",
							consequenceD = new consequence { method = "persuasion_start", parameters = new string[] {"2 2 0 2 0 0 Medium"}},
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_persuade_start_reservation",
							input = "player_need_bastards_quest_persuade_start_reservation",
							output = "player_need_bastards_quest_persuade_select_option",
							priority = 0,
							text = "{=*}I have already decided. Don't expect me to change my mind.",
							conditionD = new condition { method = "persuade_not_failed_on_condition"},

						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_persuade_select_option_0",
							input = "player_need_bastards_quest_persuade_select_option",
							output = "player_need_bastards_quest_persuade_selected_option_response",
							priority = 0,
							text = "{=!}{PERSUADE_TEXT_0}",
							conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"0"}},
							consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"0"} },
							cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"0"}},
							pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"0"}},
						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_persuade_select_option_1",
							input = "player_need_bastards_quest_persuade_select_option",
							output = "player_need_bastards_quest_persuade_selected_option_response",
							priority = 0,
							text = "{=!}{PERSUADE_TEXT_1}",
							conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"1"}},
							consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"1"} },
							cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"1"}},
							pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"1"}},
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_persuade_selected_option_response",
							input = "player_need_bastards_quest_persuade_selected_option_response",
							output = "player_need_bastards_quest_persuade_start_reservation",
							priority = 0,
							text = "{=*}{PERSUASION_REACTION}",
							conditionD = new condition { method = "persuasion_selected_option_response_on_condition"},
							consequenceD = new consequence { method = "persuasion_selected_option_response_on_consequence"},
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_persuade_rejected",
							input = "player_need_bastards_quest_persuade_start_reservation",
							output = "player_need_bastards_quest_persuade_failed",
							priority = 0,
							text = "{=!}{FAILED_PERSUASION_LINE}",
							conditionD = new condition { method = "persuade_failed_on_condition"},
							consequenceD = new consequence { method = "end_persuasion_fail_on_consequence"},
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_persuade_success",
							input = "player_need_bastards_quest_persuade_start_reservation",
							output = "player_need_bastards_quest_persuade_finished",
							priority = 0,
							text = "{=*}Here you go.",
							conditionD = new condition { method = "persuasion_success_on_condition"},
							consequenceD = new consequence { method = "set_lord_persueded_on_consequence"},
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_persuade_failed",
							input = "player_need_bastards_quest_persuade_failed",
							output = "player_need_bastards_quest_persuade_finished",
							priority = 0,
							text = "{=*}Failed"
						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_persuade_finished",
							input = "player_need_bastards_quest_persuade_finished",
							output = "close_window",
							priority = 0,
							text = "See you later",
						},
					
					}
				});

                AllDialogs.dialogs.Add(
                new Dialog
                {
                    id = "player_need_bastards_quest_discuss",
                    input = "quest_discuss",
                    priority = 125,

                    lines =
                    {
                        new Line
                        {
                            type = "npc_line",
                            id = "player_need_bastards_quest_discuss_0",
                            input = "quest_discuss",
                            output = "player_need_bastards_quest_discuss_options",
                            priority = 125,
                            text = "{=*}How is it going?[if:happy]",
                            conditionD = new condition { method = "NotableDialogCondition"}
                        },
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_discuss_1",
							input = "player_need_bastards_quest_discuss_options",
							output = "close_window",
							priority = 125,
							text = "{=*}Working on it...",
							consequenceD = new consequence { method = "leave_encounter_on_consequence"}
						},						
						new Line
                        {
                            type = "player_line",
                            id = "player_need_bastards_quest_discuss_2",
                            input = "player_need_bastards_quest_discuss_options",
                            output = "player_need_bastards_quest_discuss_3",
                            priority = 120,
                            text = "{=*}Let's finish the deal.",
							conditionD = new condition { method = "can_bartert_bastards_on_condition"},
							consequenceD = new consequence { method = "barter_the_bastard_on_consequence"}
						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_discuss_2",
							input = "player_need_bastards_quest_discuss_options",
							output = "player_need_bastards_quest_discuss_3",
							priority = 120,
							text = "{=*}Let's give me my bastard back now.",
							conditionD = new condition { method = "can_take_bastards_on_condition"},
							consequenceD = new consequence { method = "take_the_bastard_on_consequence"}
						},
						new Line
						{
							type = "npc_line",
							id = "player_need_bastards_quest_discuss_3",
							input = "player_need_bastards_quest_discuss_3",
							output = "player_need_bastards_quest_discuss_3",
							priority = 120,
							text = "{=*}So what'll be now?[ib:nervous]",
						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_discuss_optionsA",
							input = "player_need_bastards_quest_discuss_3",
							output = "start",
							priority = 120,
							text = "{=*}Celebrations.",
							conditionD = new condition { method = "check_if_quest_success"},
							consequenceD = new consequence { method = "quest_completed_on_consequence"}

						},
						new Line
						{
							type = "player_line",
							id = "player_need_bastards_quest_discuss_optionsB",
							input = "player_need_bastards_quest_discuss_3",
							output = "start",
							priority = 0,
							text = "{=*}I'll be back!",
							conditionD = new condition { method = "bastards_not_returned"}
						},
					}
                });
            }

			private void set_lord_persueded_on_consequence()
            {
				this._lordIsPersueded = true;
			}

			private bool get_lord_persueded_on_consequence()
			{
				return this._lordIsPersueded;
			}

			private bool get_lord_notpersueded_on_consequence()
			{
				return !(this._lordIsPersueded || _canBarterBastard);
			}

			private bool bastards_not_returned()
            {
				var list = Hero.MainHero.Children.Where((x) => x.Name.Contains("the bastard") && x.Clan == base.QuestGiver.Clan);

				return !list.IsEmpty();
			}

			private bool check_if_quest_success()
            {
				List<Hero> childrens = _bastardHeroes;
				
				var list = Hero.MainHero.Children.Where((x) => x.Name.Contains("the bastard") && x.Clan == base.QuestGiver.Clan);				

				foreach (var child in childrens)
				{
					int journalLogIndex = base.JournalEntries.FindIndex((x) => (x.TaskName != null ? x.TaskName.ToString() == child.Name.ToString() : false));

					if (journalLogIndex != -1 && journalLogIndex < base.JournalEntries.Count())
					{
						if (child.Clan == Clan.PlayerClan)
						{
							if (child.Age >= 18f)
							{
								MobileParty.MainParty.MemberRoster.AddToCounts(child.CharacterObject, 1, false, 0, 0, true, -1);
							}
							base.JournalEntries[journalLogIndex].UpdateCurrentProgress(1);
						}
					}
					else
					{
						InformationManager.DisplayMessage(new InformationMessage("Something wrong (have new bastard after quest was accepted). Bastard was not added to party.", Colors.Red));
					}
				}
				

               
				return list.IsEmpty();
			}

			private void barter_the_bastard_on_consequence()
			{
				List<Hero> childrens = _bastardHeroes;


				foreach (var child in childrens)
				{
					if (_canBarterBastard)
					{
						conversation_finalize_barter_consequence();
					}
				}
			}

			private void take_the_bastard_on_consequence()
			{
				List<Hero> childrens = _bastardHeroes;

				foreach (var child in childrens)
				{
					if (_lordIsPersueded)
					{
						child.Clan = Clan.PlayerClan;
					}
				}
			}

			private void quest_completed_on_consequence()
			{
				base.CompleteQuestWithSuccess();
                TextObject textObject = new TextObject("{=*} All bastard children are returned to {CLAN} clan", null);
                textObject.SetTextVariable("CLAN", Clan.PlayerClan.EncyclopediaLinkWithName);
                base.AddLog(textObject, false);
                ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, -5, false, true);
			}

			private bool persuade_not_failed_on_condition()
			{
				return !persuade_failed_on_condition() && !ConversationManager.GetPersuasionProgressSatisfied();
			}
			
			private bool persuade_failed_on_condition()
			{
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				if (currentPersuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
				{
					MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", currentPersuasionTask.FinalFailLine, false);					
					return true;
				}
				return false;
			}

			private PersuasionOptionArgs persuasion_setup_option(string index)
			{
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				return currentPersuasionTask.Options.ElementAt(int.Parse(index));
			}
			private void persuasion_start(string gv, string sv, string fv, string csv, string cfv, string ipv, string pdv)
			{
				float goalValue = float.Parse(gv);
				float successValue = float.Parse(sv);
				float failValue = float.Parse(fv);
				float criticalSuccessValue = float.Parse(csv);
				float criticalFailValue = float.Parse(cfv);
				float initialProgress = float.Parse(ipv);
				PersuasionDifficulty difficulty = (PersuasionDifficulty)Enum.Parse(typeof(PersuasionDifficulty), pdv);
				ConversationManager.StartPersuasion(goalValue, successValue, failValue, criticalSuccessValue, criticalFailValue, initialProgress, difficulty);
			}
			private void conversation_unblock_persuasion_task(string reservationType)
			{
				PersuasionTask currentPersuasionTask = _allReservations[int.Parse(reservationType)];
				currentPersuasionTask.UnblockAllOptions();
			}
			private PersuasionTask GetCurrentPersuasionTask()
			{
				foreach (PersuasionTask persuasionTask in this._allReservations)
				{
					if (!persuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked))
					{
						return persuasionTask;
					}
				}
				return this._allReservations.Last<PersuasionTask>();
			}
			
			private void end_persuasion_on_consequence()
			{
				ConversationManager.EndPersuasion();
			}

			private void end_persuasion_fail_on_consequence()
            {
				TextObject textObject = new TextObject("{=*}I failed to persuede {CONV_HERO.LINK}.", null);
				StringHelpers.SetCharacterProperties("CONV_HERO", Hero.OneToOneConversationHero.CharacterObject, textObject, false);
				this._startQuestLog = base.AddLog(textObject);
                if (!_canBarterBastard)
                {
					textObject = new TextObject("{=*} I'll need to persuede the bastard children later. Or try to imprison the mother and talk to her again.", null);
					base.AddLog(textObject, false);
				}
			}
			private bool persuasion_selected_option_response_on_condition()
			{
				PersuasionOptionResult item = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item2;

				string selectedLine = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item1.Line.ToString();

				GetOptionReaction(selectedLine, out int i, out int j, out int k, out bool found);

				if (found)
				{
					reaction[] reactions = AllDialogs.dialogs[i].Tasks[j].Options[k].optionReactions;
					string response = reactions[reactions.FindIndex((x) => ((PersuasionOptionResult)Enum.Parse(typeof(PersuasionOptionResult), x.persuasionOptionResult)) == item)].text;
					MBTextManager.SetTextVariable("PERSUASION_REACTION", response, false);
				}
				else
				{
					if ((item != PersuasionOptionResult.Failure && item != PersuasionOptionResult.CriticalFailure) || this.GetCurrentPersuasionTask().ImmediateFailLine == null)
					{
						MBTextManager.SetTextVariable("PERSUASION_REACTION", PersuasionHelper.GetDefaultPersuasionOptionReaction(item), false);
					}
				}

				if ((item == PersuasionOptionResult.Failure || item == PersuasionOptionResult.CriticalFailure) && this.GetCurrentPersuasionTask().ImmediateFailLine != null)
				{
					MBTextManager.SetTextVariable("PERSUASION_REACTION", this.GetCurrentPersuasionTask().ImmediateFailLine, false);
				}

				if (item == PersuasionOptionResult.CriticalFailure)
				{
					foreach (PersuasionTask persuasionTask in this._allReservations)
					{
						persuasionTask.BlockAllOptions();
					}
				}
				return true;
			}

			private void GetOptionReaction(string selectedLine, out int i, out int j, out int k, out bool found)
			{
				i = -1;
				j = -1;
				k = -1;
				found = false;
				for (i = 0; i < AllDialogs.dialogs.Count && !found; i++)
				{
					for (j = 0; j < AllDialogs.dialogs[i].Tasks.Count && !found; j++)
					{
						for (k = 0; k < AllDialogs.dialogs[i].Tasks[j].Options.Count; k++)
						{
							if (AllDialogs.dialogs[i].Tasks[j].Options[k].line.Contains(selectedLine))
							{
								found = AllDialogs.dialogs[i].Tasks[j].Options[k].optionReactions != null;
								return;
							}
						}
					}
				}
			}
			private void conversation_unblock_all_persuasion_task()
			{
				foreach (PersuasionTask persuasionTask in this._allReservations)
				{
					persuasionTask.UnblockAllOptions();
				}
			}
			private bool persuasion_select_option_on_condition(string index)
			{
				int i = int.Parse(index);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				if (currentPersuasionTask.Options.Count<PersuasionOptionArgs>() > i)
				{
					TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
					textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(currentPersuasionTask.Options.ElementAt(i), false));
					textObject.SetTextVariable("PERSUASION_OPTION_LINE", currentPersuasionTask.Options.ElementAt(i).Line);
					MBTextManager.SetTextVariable($"PERSUADE_TEXT_{i}", textObject, false);
					return true;
				}
				return false;
			}
			private bool persuasion_clickable_option_on_condition(out TextObject hintText, string[] index)
			{
				int i = int.Parse(index[0]);
				hintText = new TextObject("{=9ACJsI6S}Blocked", null);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				if (currentPersuasionTask.Options.Any<PersuasionOptionArgs>())
				{
					if (currentPersuasionTask.Options.Count > i)
					{
						hintText = (currentPersuasionTask.Options.ElementAt(i).IsBlocked ? hintText : TextObject.Empty);
						return !currentPersuasionTask.Options.ElementAt(i).IsBlocked;
					}
				}
				return false;
			}
			private void persuasion_block_select_option_on_consequence(string reservationType)
			{
				int i = int.Parse(reservationType);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				if (currentPersuasionTask.Options.Count > i)
				{
					currentPersuasionTask.Options[i].BlockTheOption(true);
				}
			}
			private void persuasion_selected_option_response_on_consequence()
			{
				Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>();
				float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty(PersuasionDifficulty.Hard);
				float moveToNextStageChance;
				float blockRandomOptionChance;
				Campaign.Current.Models.PersuasionModel.GetEffectChances(tuple.Item1, out moveToNextStageChance, out blockRandomOptionChance, difficulty);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				currentPersuasionTask.ApplyEffects(moveToNextStageChance, blockRandomOptionChance);

				string selectedLine = tuple.Item1.Line.ToString();
				if (selectedLine.Contains("barter") && 
					(tuple.Item2 == PersuasionOptionResult.Success || tuple.Item2 == PersuasionOptionResult.CriticalSuccess))
				{
					_canBarterBastard = true;
				}
			}

			private bool can_bartert_bastards_on_condition()
            {
				return _canBarterBastard;
            }

			private bool can_take_bastards_on_condition()
			{
				return (!_canBarterBastard && _lordIsPersueded);
			}

			private void conversation_finalize_barter_consequence()
			{
				Hero heroBeingProposedTo = Hero.OneToOneConversationHero;
				Barterable[] barterables = new Barterable[_bastardHeroes.Count];

				for( int i = 0; i < _bastardHeroes.Count; i++)
				{
                    barterables[i] = new BastardBarterable(Hero.OneToOneConversationHero, PartyBase.MainParty, _bastardHeroes[i], Hero.MainHero);
				}
				
				BarterManager instance = BarterManager.Instance;
				Hero mainHero = Hero.MainHero;
				Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
				PartyBase mainParty = PartyBase.MainParty;
				MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;
				instance.StartBarterOffer(mainHero, oneToOneConversationHero, mainParty, (partyBelongedTo != null) ? partyBelongedTo.Party : null, null,null , 0, false, barterables);				
			}

			private bool persuasion_success_on_condition()
			{
				return ConversationManager.GetPersuasionProgressSatisfied();
			}
			#endregion

			protected override void SetDialogs()
			{
				string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PlayerNeedsBastardsIssueQuest" + "Lines.xml");
				AllDialogs = new DialogHelper();
				if (!File.Exists(file))
				{
					AddDefault();
					AllDialogs.Save(file);
				}
				AllDialogs.Load(file);
				_allReservations = AllDialogs.GetAllDialogTasks();
				var dialogFlows = AllDialogs.GetDialogFlows(this);

				int i = 0;
				foreach (var dialogFlow in dialogFlows)
				{
					if (AllDialogs.dialogs[i].input != "issue_classic_quest_start" && AllDialogs.dialogs[i].input != "quest_discuss")
					{
						Campaign.Current.ConversationManager.AddDialogFlow(dialogFlow, this);
					}
				}

				this.OfferDialogFlow = dialogFlows[AllDialogs.dialogs.FindIndex((x) => x.input == "issue_classic_quest_start")];
				this.DiscussDialogFlow = dialogFlows[AllDialogs.dialogs.FindIndex((x) => x.input == "quest_discuss")];
			}

		}

		public class PlayerNeedsBastardsIssueTypeDefiner : SaveableTypeDefiner
		{
			public PlayerNeedsBastardsIssueTypeDefiner() : base(2023031060)
			{
			}

			protected override void DefineClassTypes()
			{
				base.AddClassDefinition(typeof(PlayerNeedsBastardsIssue), 1, null);
				base.AddClassDefinition(typeof(PlayerNeedsBastardsIssueQuest), 2, null);
			}
		}

	}
}
