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
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BastardsMod
{
    internal class LordNeedBastardBackIssueBehavior : CampaignBehaviorBase
	{

		public override void RegisterEvents()
		{
			CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener(this, new Action<Hero>(this.OnCheckForIssue));
		}
		
        public override void SyncData(IDataStore dataStore)
		{

		}

		public void OnCheckForIssue(Hero hero)
		{
			if (this.ConditionsHold(hero))
			{
				Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new PotentialIssueData.StartIssueDelegate(this.OnStartIssue),
					typeof(LordNeedsBastardsIssue), IssueBase.IssueFrequency.VeryCommon));
				return;
			}
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(
				typeof(LordNeedsBastardsIssue), IssueBase.IssueFrequency.VeryCommon));
		}

		private bool SuitableCondition(Hero hero)
		{
			return hero.Age >= 18f && hero.IsLord && hero.IsActive /*&& !hero.IsFemale*/ && HaveBastardsNotInHeroClan(hero);
		}

		private bool ConditionsHold(Hero hero)
		{
			return this.SuitableCondition(hero);
		}

		private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
		{
			PotentialIssueData potentialIssueData = pid;
			return new LordNeedsBastardsIssue(issueOwner, potentialIssueData);
		}
		private List<Hero> GetBastardsNotInHeroClan(Hero familyMember, Clan clan)
		{
			List<Hero> list = familyMember.Children.Where((x) => x.Name.Contains("the bastard") &&
			x.Clan != clan).ToList();
			return list;
		}

		private bool HaveBastardsNotInHeroClan(Hero familyMember)
		{
			List<Hero> childrens = GetBastardsNotInHeroClan(familyMember, familyMember.Clan);
			if (childrens != null && childrens.Count > 0)
				return true;
			return false;
		}

		public class LordNeedsBastardsIssue : IssueBase
		{
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
			public override TextObject IssueBriefByIssueGiver
			{
				get
				{
					TextObject textObject = new TextObject("{=*}I've heard good things about you. They say that you are an charming leader. I have a proposal for you. I had some unexpected adventures with laydies... and so in the end I have bastard children.", null);
					return textObject;
				}
			}
			public override TextObject IssueAcceptByPlayer
			{
				get
				{
					return new TextObject("{=a1n2zCaD}What exactly do you wish from me?", null);
				}
			}
			public override TextObject IssueQuestSolutionExplanationByIssueGiver
			{
				get
				{
					return new TextObject("{=*}Return my bastard children to my clan. What do you say?", null);
				}
			}
			public override TextObject IssueQuestSolutionAcceptByPlayer
			{
				get
				{
					return new TextObject("{=*}All right.", null);
				}
			}
			public override TextObject Title
			{
				get
				{
					TextObject textObject = new TextObject("{=*}{?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} needs bastard children back", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject, false);
					return textObject;
				}
			}
			public override TextObject Description
			{
				get
				{
					TextObject textObject = new TextObject("{=*}{QUEST_GIVER.NAME}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of the {CLAN}, wants bastard children return to {CLAN} clan.", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.IssueOwner.CharacterObject, textObject, false);
					textObject.SetTextVariable("CLAN", base.IssueOwner.Clan.Name);
					return textObject;
				}
			}
			public LordNeedsBastardsIssue(Hero issueOwner, PotentialIssueData potentialIssueData) : base(issueOwner, CampaignTime.DaysFromNow(200f))
			{
			}
			protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
			{
				if (issueEffect == DefaultIssueEffects.ClanInfluence)
				{
					return -0.01f;
				}
				return 0f;
			}

			protected override void OnGameLoad()
			{
			}

			protected override QuestBase GenerateIssueQuest(string questId)
			{
				return new LordNeedsBastardsIssueQuest(questId, base.IssueOwner);
			}

			public override IssueBase.IssueFrequency GetFrequency()
			{
				return IssueBase.IssueFrequency.Common;
			}

			protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out IssueBase.PreconditionFlags flag, out Hero relationHero, out SkillObject skill)
			{
				relationHero = null;
				skill = null;
				flag = IssueBase.PreconditionFlags.None;
				if (issueGiver.GetRelationWithPlayer() < -10f)
				{
					flag |= IssueBase.PreconditionFlags.Relation;
					relationHero = issueGiver;
				}
				if (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
				{
					flag |= IssueBase.PreconditionFlags.AtWar;
				}
                return flag == IssueBase.PreconditionFlags.None;
			}

			public override bool IssueStayAliveConditions()
			{
				return base.IssueOwner.Clan != Clan.PlayerClan && base.IssueOwner.IsAlive;
			}

			protected override void CompleteIssueWithTimedOutConsequences()
			{
			}

			[SaveableField(11)]
			private List<Hero> _bastardHeroes;
		}

		public class LordNeedsBastardsIssueQuest : QuestBase
		{
			[SaveableField(10)]
			private List<Hero> _bastardHeroes;

			[SaveableField(20)]
			private bool _checkForMissionEnd;

			[SaveableField(30)]
			private bool _firstConversationInitialized;

			[SaveableField(60)]
			private int _randomForQuestReward;

			[SaveableField(80)]
			private JournalLog _startQuestLog;
			
			public override bool IsRemainingTimeHidden
			{
				get
				{
					return false;
				}
			}

			#region DIALOGS
			
			private bool _isLordPersuaded;
			private List<PersuasionTask> _allReservations = new List<PersuasionTask>();
			private DialogHelper AllDialogs = new DialogHelper();

			public void AddDefault()
			{
				AllDialogs.dialogs.Add(
				new Dialog
				{
					id = "need_bastards_quest_start",
					input = "issue_classic_quest_start",
					priority = 125,

					lines =
					{
						new Line
						{
							type = "npc_line",
							id = "need_bastards_quest_start_0",
							input = "issue_classic_quest_start",
							output = "need_bastards_quest_start_1",
							priority = 125,
							text = "{=*}Wery good, start now![if:happy]",
							conditionD = new condition { method = "NotableDialogCondition"},
							consequenceD = new consequence { method = "QuestAcceptedConsequences"}
						},
						new Line
						{
							type = "player_line",
							id = "need_bastards_quest_start_1",
							input = "need_bastards_quest_start_1",
							output = "close_window",
							priority = 125,
							text = "{=*}I am on my way!"
						}
					}
				});
				AllDialogs.dialogs.Add(
				new Dialog
				{
					id = "need_bastards_quest_discuss",
					input = "quest_discuss",
					priority = 125,

					lines =
					{
						new Line
						{
							type = "npc_line",
							id = "need_bastards_quest_discuss_0",
							input = "quest_discuss",
							output = "need_bastards_quest_discuss_options",
							priority = 125,
							text = "{=*}How is the task progress going?[if:happy]",
							conditionD = new condition { method = "NotableDialogCondition"}
						},
						new Line
						{
							type = "player_line",
							id = "need_bastards_quest_discuss_1",
							input = "need_bastards_quest_discuss_options",
							output = "close_window",
							priority = 125,
							text = "{=*}Working on it...",
							consequenceD = new consequence { method = "leave_encounter_on_consequence"}
						},
						new Line
						{
							type = "player_line",
							id = "need_bastards_quest_discuss_2",
							input = "need_bastards_quest_discuss_options",
							output = "need_bastards_quest_discuss_3",
							priority = 125,
							text = "{=*}Just fine! I have somebody for you.",
							conditionD = new condition { method = "leave_bastards_to_lord_on_condition"},
							consequenceD = new consequence { method = "leave_bastards_to_lord_on_consequence"}
						},
						new Line
						{
							type = "npc_line",
							id = "need_bastards_quest_discuss_3",
							input = "need_bastards_quest_discuss_3",
							output = "close_window",
							priority = 125,
							text = "{=*}Very good![if:happy]",
							consequenceD = new consequence { method = "leave_encounter_on_consequence"}
						},
					}
				});
				AllDialogs.dialogs.Add(
				new Dialog
				{
					id = "need_bastards_quest_completed",
					input = "start",
					priority = 125,

					lines =
					{
						new Line
						{
							type = "npc_line",
							id = "need_bastards_quest_completed_0",
							input = "start",
							output = "need_bastards_quest_completed_1",
							priority = 125,
							text = "{=*}Greetings, my {?PLAYER.GENDER}lady{?}lord{\\?}.I knew it![if:happy]",
							conditionD = new condition { method = "quest_completed_on_condition"}
						},
						new Line
						{
							type = "player_line",
							id = "need_bastards_quest_completed_1",
							input = "need_bastards_quest_completed_1",
							output = "close_window",
							priority = 125,
							text = "{=*}You are welcome.",
							consequenceD = new consequence { method = "quest_completed_on_consequence" }
						}
					}
				});
				AllDialogs.dialogs.Add(
				new Dialog
				{
					id = "give_need_bastards_quest",
					input = "start",
					priority = 125,

					lines =
					{
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_0",
											input = "start",
											output = "give_need_bastards_quest_1",
											priority = 125,
											text = "{=*}Greetings, my {?PLAYER.GENDER}lady{?}lord{\\?}.[ib:nervous][if:convo_grave]",											
											conditionD = new condition { method = "return_bastard_on_condition"}
										},
										new Line
										{
											type = "player_line",
											id = "give_need_bastards_quest_1",
											input = "give_need_bastards_quest_1",
											output = "give_need_bastards_quest_2",
											priority = 125,
											text = "{=*}I'm here for {QUEST_GIVER}'s bastards.",
											conditionD = new condition { method = "return_bastard_ask_for_bastards_on_condition"},
											
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_2mlt0",
											input = "give_need_bastards_quest_2",
											output = "give_bastards_quest_persuade_start",
											priority = 125,
											text = "{=*}No {?PLAYER.GENDER}my lady{?}sir{\\?}! I have arguments!.[ib:aggressive][if:convo_grave]",
											conditionD = new condition { method = "hero_trait_on_condition",  parameters = new string[] { "Mercy < 0" } },
											consequenceD = new consequence { method = "conversation_unblock_persuasion_task", parameters = new string[] { "0" } },
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_2mgt0",
											input = "give_need_bastards_quest_2",
											output = "give_bastards_quest_persuade_start",
											priority = 125,
											text = "{=*}Yes {?PLAYER.GENDER}my lady{?}sir{\\?}! I guess I have to help...[ib:nervous][if:convo_grave]",
											conditionD = new condition { method = "hero_trait_on_condition",  parameters = new string[] { "Mercy > 0" } },
											consequenceD = new consequence { method = "conversation_unblock_persuasion_task", parameters = new string[] { "0" } },
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_2glt0",
											input = "give_need_bastards_quest_2",
											output = "give_bastards_quest_persuade_start",
											priority = 125,
											text = "{=*}No {?PLAYER.GENDER}my lady{?}sir{\\?}! I need a favor too!.[ib:aggressive][if:convo_grave]",
											conditionD = new condition { method = "hero_trait_on_condition",  parameters = new string[] { "Generosity < 0" } },
											consequenceD = new consequence { method = "conversation_unblock_persuasion_task", parameters = new string[] { "0" } },
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_2glt0",
											input = "give_need_bastards_quest_2",
											output = "give_bastards_quest_persuade_start",
											priority = 125,
											text = "{=*}No {?PLAYER.GENDER}my lady{?}sir{\\?}! No one did favors to me!.[ib:aggressive][if:convo_grave]",
											conditionD = new condition { method = "hero_trait_on_condition",  parameters = new string[] { "Generosity > 0" } },
											consequenceD = new consequence { method = "conversation_unblock_persuasion_task", parameters = new string[] { "0" } },
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_2_default",
											input = "give_need_bastards_quest_2",
											output = "give_need_bastards_quest_3",
											priority = 0,
											text = "{=*}Yes {?PLAYER.GENDER}my lady{?}sir{\\?}...[ib:aggressive][if:convo_grave]",
										},
										new Line
										{
											type = "npc_line",
											id = "give_need_bastards_quest_3",
											input = "give_need_bastards_quest_3",
											output = "close_window",
											priority = 125,
											text = "{=*}Hrr...[ib:closed][if:convo_bared_teeth]",
											consequenceD = new consequence { method = "return_bastard_on_consequence"},
										},
					}
				});

				AllDialogs.dialogs.Add(new Dialog
				{
					id = "give_bastards_quest_persuade",
					input = "give_bastards_quest_persuade_start",
					priority = 125,

					Tasks =
				{
					new persuasionTask
					{
						 id = "give_bastards_quest_persuade_task_0",
						 spokenLine = "{=*}",
						 finalFailLine = "{=*}Guards![if:convo_bared_teeth][if:idle_angry]",
						 tryLaterLine = "{=*}I'm sorry, not today.[ib:nervous][ib:closed]",
						 Options =
						 {
							new persuasionOption
							{
								line = "{=*}Nothing will stop me from doing my duty!",
								defaultSkills = "Leadership",
								defaultTraits = "Generosity",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = false,
								traitCorrelations = new traitCorrelations { valor = 0,  mercy = 0,  honor = 0,  generosity = 2,  calculating = 0 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] { 
									new reaction { persuasionOptionResult = "Success", text = "Ok, duty is important."},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "I agree.[ib:happy]" }, 
									new reaction { persuasionOptionResult = "Failure", text = "How about duty to me?[ib:nervous][ib:closed]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "Duty to my ass![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
								}
							},
							new persuasionOption
							{
								line = "{=*}Bring it on!",
								defaultSkills = "Leadership",
								defaultTraits = "Valor",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = false,
								traitCorrelations = new traitCorrelations { valor = 1,  mercy = 0,  honor = -1,  generosity = 0,  calculating = -2 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] {
									new reaction { persuasionOptionResult = "Success", text = "Ok I will.[rf:convo_relaxed_happy]"},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "Good, I dont need him anyway.[ib:happy][ib:normal]" },
									new reaction { persuasionOptionResult = "Failure", text = "No deal![if:idle_angry]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "Never![ib:nervous][ib:closed][if:convo_bared_teeth]" },
								}
							}
							
						 },
						immediateFailLine = "",
						reservationType = 0
					},
					new persuasionTask
					{
						 id = "give_bastards_quest_persuade_task_1",
						 spokenLine = "{=*}",
						 finalFailLine = "{=*}Guards!Guards![ib:nervous][ib:closed][if:convo_bared_teeth]",
						 tryLaterLine = "{=*}I'm sorry.[ib:nervous][ib:closed]",
						 Options =
						 {
							new persuasionOption
							{
								line = "{=*}Get your ass ready!",
								defaultSkills = "Leadership",
								defaultTraits = "Generosity",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = true,
								traitCorrelations = new traitCorrelations { valor = 1,  mercy = -2,  honor = -2,  generosity = -2,  calculating = 0 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] {
									new reaction { persuasionOptionResult = "Success", text = "A good idea.[ib:happy][ib:normal]"},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "That make a lot of sense.[ib:normal][ib:closed]" },
									new reaction { persuasionOptionResult = "Failure", text = "No way![if:idle_angry]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "Just no. Get lost![ib:closed][if:convo_bared_teeth]" },
								}
							},
							new persuasionOption
							{
								line = "{=*}You will die for this!",
								defaultSkills = "Leadership",
								defaultTraits = "Valor",
								traitEffect = "Positive",
								persuasionArgumentStrength = "Hard",
								givesCriticalSuccess = false,
								traitCorrelations = new traitCorrelations { valor = 2,  mercy = -2,  honor = -2,  generosity = -2,  calculating = -2 },
								canBlockOtherOption = false,
								canMoveToTheNextReservation = false,
								isInitiallyBlocked  = false,
								optionReactions = new reaction[] {
									new reaction { persuasionOptionResult = "Success", text = "Ok I will give.[rf:convo_relaxed_happy]"},
									new reaction { persuasionOptionResult = "CriticalSuccess", text = "Good, I dont need him anyway.[ib:happy][ib:normal]" },
									new reaction { persuasionOptionResult = "Failure", text = "No deal![if:idle_angry]" },
									new reaction { persuasionOptionResult = "CriticalFailure", text = "Never![ib:nervous][ib:closed][if:convo_bared_teeth]" },
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
                        id = "give_bastards_quest_persuade_start_introduction",
                        input = "give_bastards_quest_persuade_start",
                        output = "give_bastards_quest_persuade_start_reservation",
                        priority = 0,
                        text = "{=*}What is it?",
						consequenceD = new consequence { method = "persuasion_start", parameters = new string[] {"3 2 0 2 0 0 Medium"}},
					},
					new Line
					{
						type = "npc_line",
						id = "give_bastards_quest_persuade_rejected",
						input = "give_bastards_quest_persuade_start_reservation",
						output = "give_bastards_quest_persuade_failed",
						priority = 0,
						text = "{=!}{FAILED_PERSUASION_LINE}",
						conditionD = new condition { method = "persuade_failed_on_condition"},
						consequenceD = new consequence { method = "end_persuasion_on_consequence"},
					},
                    new Line
					{
						type = "npc_line",
						id = "give_bastards_quest_persuade_start_reservation",
						input = "give_bastards_quest_persuade_start_reservation",
						output = "give_bastards_quest_persuade_select_option",
						priority = 0,
						text = "{=*}I have already decided. Don't expect me to change my mind.",
						conditionD = new condition { method = "persuade_not_failed_on_condition"},
						
					},
					new Line
					{
						type = "player_line",
						id = "give_bastards_quest_persuade_select_option_0",
						input = "give_bastards_quest_persuade_select_option",
						output = "give_bastards_quest_persuade_selected_option_response",
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
						id = "give_bastards_quest_persuade_select_option_1",
						input = "give_bastards_quest_persuade_select_option",
						output = "give_bastards_quest_persuade_selected_option_response",
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
						id = "give_bastards_quest_persuade_selected_option_response",
						input = "give_bastards_quest_persuade_selected_option_response",
						output = "give_bastards_quest_persuade_start_reservation",
						priority = 0,
						text = "{=*}{PERSUASION_REACTION}",
						conditionD = new condition { method = "persuasion_selected_option_response_on_condition"},
						consequenceD = new consequence { method = "persuasion_selected_option_response_on_consequence"},
					},
					new Line
					{
						type = "npc_line",
						id = "give_bastards_quest_persuade_failed",
						input = "give_bastards_quest_persuade_failed",
						output = "give_bastards_quest_persuade_finished",
						priority = 0,
						text = "{=*}Failed"
					},
					new Line
					{
						type = "npc_line",
						id = "give_bastards_quest_persuade_success",
						input = "give_bastards_quest_persuade_start_reservation",
						output = "give_bastards_quest_persuade_finished",
						priority = 0,
						text = "{=*}You're right. I will do as you say.",
						conditionD = new condition { method = "persuasion_success_on_condition"},
						consequenceD = new consequence { method = "return_bastard_on_consequence"},
					},
					 new Line
					{
						type = "player_line",
						id = "give_bastards_quest_persuade_finished",
						input = "give_bastards_quest_persuade_finished",
						output = "close_window",
						priority = 0,
						text = "The end.",
					}
					}
                });
			}

			//"Mercy represents your general aversion to suffering and your willingness to help strangers or even enemies."
			//"Valor represents your reputation for risking your life to win glory or wealth or advance your cause."
			//"Honor represents your reputation for respecting your formal commitments, like keeping your word and obeying the law."
			//"Generosity represents your loyalty to your kin and those who serve you, and your gratitude to those who have done you a favor."
			//"Calculating represents your ability to control your emotions for the sake of your long-term interests."

			#endregion
			public override TextObject Title
			{
				get
				{
					TextObject textObject = new TextObject("{=*}{?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} needs bastard children back", null);
					StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
					return textObject;
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
			public LordNeedsBastardsIssueQuest(string questId, Hero questGiver) : base(questId, questGiver, CampaignTime.DaysFromNow(200f), 0)
			{
				this._firstConversationInitialized = false;
				this._randomForQuestReward = MBRandom.RandomInt(2, 5);
				this._bastardHeroes = questGiver.Children.Where((x) => x.Name.Contains("the bastard") && x.Clan != questGiver.Clan).ToList();
				this.SetDialogs();
				base.InitializeQuestOnCreation();
			}
			protected override void InitializeQuestOnGameLoad()
			{
				this.SetDialogs();
			}
			protected override void SetDialogs()
			{
				string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LordNeedsBastardsIssueQuest" + "Lines.xml");
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

			/// <summary>
			/// Gets task starting from list start with any options not blocked, or last task.
			/// </summary>
			/// <returns></returns>
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

			private void conversation_unblock_persuasion_task(string reservationType)
			{
				PersuasionTask currentPersuasionTask =  _allReservations[int.Parse(reservationType)];
				currentPersuasionTask.UnblockAllOptions();
			}

			private void conversation_unblock_current_persuasion_task()
			{
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				currentPersuasionTask.UnblockAllOptions();
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

			private void persuasion_block_select_option_on_consequence(string reservationType)
			{
				int i = int.Parse(reservationType);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				if (currentPersuasionTask.Options.Count > i)
				{
					currentPersuasionTask.Options[i].BlockTheOption(true);
				}
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

			private PersuasionOptionArgs persuasion_setup_option(string index)
			{
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				return currentPersuasionTask.Options.ElementAt(int.Parse(index));
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
			private void persuasion_selected_option_response_on_consequence()
			{
				Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>();
				float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty(PersuasionDifficulty.Hard);
				float moveToNextStageChance;
				float blockRandomOptionChance;
				Campaign.Current.Models.PersuasionModel.GetEffectChances(tuple.Item1, out moveToNextStageChance, out blockRandomOptionChance, difficulty);
				PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
				currentPersuasionTask.ApplyEffects(moveToNextStageChance, blockRandomOptionChance);
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
			private void end_persuasion_on_consequence()
			{
				ConversationManager.EndPersuasion();
			}

			private bool persuasion_success_on_condition()
			{
				return ConversationManager.GetPersuasionProgressSatisfied();
			}

			private bool persuade_not_failed_on_condition()
			{
				return !persuade_failed_on_condition() && !persuasion_success_on_condition();
			}

			private bool hero_trait_on_condition(string tag, string operation, string weight)
            {
				if (Hero.OneToOneConversationHero == null) return false;

                int value = GetHeroTraitValue(Hero.OneToOneConversationHero, tag);

                bool result = operation == "==" ? value == int.Parse(weight) :
                              operation == ">=" ? value >= int.Parse(weight) :
                              operation == "<=" ? value <= int.Parse(weight) :
                              operation == ">" ? value > int.Parse(weight) :
                              operation == "<" ? value < int.Parse(weight) :
                              operation == "!=" ? value != int.Parse(weight) : false;
                return result;
            }

            private static int GetHeroTraitValue(Hero hero, string tag)
            {
                CharacterTraits ht = hero.GetHeroTraits();
                PropertyInfo[] props = ht.GetType().GetProperties();
                var prop = props.Where((x) => x.Name == tag).FirstOrDefault();
				if (prop == null) 
				{
					return 0;
				}
                
				int value = (int)prop.GetValue((object)ht);

                return value;
            }

			private bool return_bastard_ask_for_bastards_on_condition()
            {
				MBTextManager.SetTextVariable("QUEST_GIVER", base.QuestGiver.Name.ToString(), false);
				return true;
            }

			private bool return_bastard_on_condition()
			{
				if (Hero.OneToOneConversationHero == base.QuestGiver || Hero.OneToOneConversationHero == null) return false;

				List<Hero> childrens = Hero.OneToOneConversationHero.Children.Where((x) => x.Name.Contains("the bastard") &&
				x.Father == base.QuestGiver && x.Clan == x.Mother.Clan).ToList();
				bool have_bastards = childrens != null && childrens.Count > 0;
				return have_bastards;
			}

			private void return_bastard_on_consequence()
			{
				List<Hero> childrens = Hero.OneToOneConversationHero.Children.Where((x) => x.Name.Contains("the bastard") &&
				x.Father == base.QuestGiver).ToList();

				foreach (var child in childrens)
				{
					int journalLogIndex = base.JournalEntries.FindIndex((x) => (x.TaskName != null ? x.TaskName.ToString() == child.Name.ToString() : false));
					if (journalLogIndex != -1 && journalLogIndex < base.JournalEntries.Count())
					{
						//child.Clan = base.QuestGiver.Clan;
						if (child.Age >= 18f)
						{
							AddCompanionAction.Apply(base.QuestGiver.Clan, child);
							MobileParty.MainParty.MemberRoster.AddToCounts(child.CharacterObject, 1, false, 0, 0, true, -1);
						}
						else
						{
							child.Clan = base.QuestGiver.Clan;
						}
						base.JournalEntries[journalLogIndex].UpdateCurrentProgress(1);
						ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, -5, false, true);
						ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.OneToOneConversationHero, base.QuestGiver, -10, true);
					}
					else
					{
						InformationManager.DisplayMessage(new InformationMessage("Something wrong (have new bastard after quest was accepted). Bastard was not added to party.", Colors.Red));
					}
				}
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
			}

			private void leave_encounter_on_consequence()
            {
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
			}

			private bool quest_completed_on_condition()
			{
				if (Hero.OneToOneConversationHero != base.QuestGiver || Hero.OneToOneConversationHero == null) return false;
				if (base.JournalEntries == null || base.JournalEntries.Count == 0)
					return false;
				int journalCounter = base.JournalEntries.Where((x) => (x.CurrentProgress == 0)).Count();
				return journalCounter == 1;
			}

			private bool leave_bastards_to_lord_on_condition()
            {
				bool haveBasttardsInParty = false;
				for (int i = 0; i < this._bastardHeroes.Count; i++)
				{
					if (MobileParty.MainParty.MemberRoster.Contains(this._bastardHeroes[i].CharacterObject))
					{
						haveBasttardsInParty = true;
						break;
					}
				}

				return haveBasttardsInParty;
            }

			private void leave_bastards_to_lord_on_consequence()
            {
				int i = 0;
				for (; i < this._bastardHeroes.Count; i++)
				{
					if (MobileParty.MainParty.MemberRoster.Contains(this._bastardHeroes[i].CharacterObject))
					{
						MobileParty.MainParty.MemberRoster.RemoveTroop(this._bastardHeroes[i].CharacterObject, 1);
						ChangeRelationAction.ApplyPlayerRelation(this._bastardHeroes[i], 10, false, true);
					}
				}
				this.RelationshipChangeWithQuestGiver = 5*i;
				ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, false, true);
            }

			private void quest_completed_on_consequence()
			{
				for (int i = 0; i < this._bastardHeroes.Count; i++)
				{
					//this._bastardHeroes[i].Clan = base.QuestGiver.Clan;

					if (this._bastardHeroes[i].Age >= (new DefaultAgeModel()).HeroComesOfAge)
					{
						//RemoveCompanionAction.ApplyAfterQuest(Clan.PlayerClan, this._bastardHeroes[i]);
						//AddCompanionAction.Apply(base.QuestGiver.Clan, this._bastardHeroes[i]);
						MobileParty.MainParty.MemberRoster.RemoveTroop(this._bastardHeroes[i].CharacterObject, 1);
					}
					//else
					//{
					//                   this._bastardHeroes[i].Clan = base.QuestGiver.Clan;
					//}

					ChangeRelationAction.ApplyPlayerRelation(this._bastardHeroes[i], 10, false, true);
				}
				this.RelationshipChangeWithQuestGiver = 25;
				ChangeRelationAction.ApplyPlayerRelation(base.QuestGiver, this.RelationshipChangeWithQuestGiver, false, true);
				base.CompleteQuestWithSuccess();

				TextObject textObject = new TextObject("{=*} All bastard children are returned to {CLAN} clan", null);
				textObject.SetTextVariable("CLAN", base.QuestGiver.Clan.EncyclopediaLinkWithName);
				base.AddLog(textObject, false);
				ItemObject randomElementWithPredicate = Items.All.GetRandomElementWithPredicate((ItemObject x) => x.IsTradeGood && x.ItemCategory == DefaultItemCategories.Jewelry);
				textObject = new TextObject("{=lWvuM5aj}{GIFT_NUMBER} pieces of {JEWELRY} have been added to your inventory.", null);
				textObject.SetTextVariable("GIFT_NUMBER", this._randomForQuestReward);
				textObject.SetTextVariable("JEWELRY", randomElementWithPredicate.Name);
				MBInformationManager.AddQuickInformation(textObject, 0, null, "");
				base.AddLog(textObject, false);
				MobileParty.MainParty.ItemRoster.AddToCounts(randomElementWithPredicate, this._randomForQuestReward);
				GainRenownAction.Apply(Hero.MainHero, 5f, false);
				if (PlayerEncounter.Current != null)
				{
					PlayerEncounter.LeaveEncounter = true;
				}
			}

			private bool NotableDialogCondition()
			{
				return Hero.OneToOneConversationHero == base.QuestGiver;
			}

			private void QuestAcceptedConsequences()
			{
				base.StartQuest();
				TextObject PlayerStartsQuestLogText = new TextObject("{=*}{QUEST_GIVER.LINK}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} of the {CLAN}, wants bastard children return to clan in {DURATION} days. You have accepted to persuade them to return.", null);
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
				TextObject textObject = new TextObject("{=taz5cAtw}You failed to return bastards to {QUEST_GIVER.LINK}.", null);
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

			private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
			{
				if (base.QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
				{
					base.CompleteQuestWithCancel(this.WarDeclaredQuestCancel);
				}
			}

			private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarAction.DeclareWarDetail detail)
			{
				TextObject textObject = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.", null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", base.QuestGiver.CharacterObject, textObject, false);
				QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest(this, faction1, faction2, detail, textObject, this.WarDeclaredQuestCancel);
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
		}

		public class LordNeedsBastardsIssueTypeDefiner : SaveableTypeDefiner
		{
			public LordNeedsBastardsIssueTypeDefiner() : base(2023021200)
			{
			}

			protected override void DefineClassTypes()
			{
				base.AddClassDefinition(typeof(LordNeedsBastardsIssue), 1, null);
				base.AddClassDefinition(typeof(LordNeedsBastardsIssueQuest), 2, null);
			}
		}
	}
}
