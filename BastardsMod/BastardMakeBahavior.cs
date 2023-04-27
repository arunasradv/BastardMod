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
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BastardsMod
{
    internal class BastardMakeBahavior : CampaignBehaviorBase
    {
        private List<PersuasionTask> _allReservations = new List<PersuasionTask>();
        private DialogHelper AllDialogs = new DialogHelper();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunchedEvent));
        }

        public override void SyncData(IDataStore dataStore)
        {
            
        }

        public void OnSessionLaunchedEvent(CampaignGameStarter starter)
        {
            string file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BastardMakeBahaviorLines.xml");
            AllDialogs = new DialogHelper();
           
            if (!File.Exists(file))
            {
                AddDefault();
                AllDialogs.Save(file);
            }

            AllDialogs.Load(file);
            AllDialogs.LoadDialogs(starter, this);
            _allReservations = AllDialogs.GetAllDialogTasks();
            var dialogFlows = AllDialogs.GetDialogFlows(this);

            int i = 0;
            foreach (var dialogFlow in dialogFlows)
            {
                    Campaign.Current.ConversationManager.AddDialogFlow(dialogFlow, this);
            }
        }
 #region DIALOG
        public void AddDefault()
        {
            //Starting dialog line
            AllDialogs.lines.Add(
                new Line
                {
                    type = "player_line",
                    id = "to_make_bastard_persuasion_0",
                    input = "lord_talk_speak_diplomacy_2",
                    output = "start_persuade_to_make_bastard_persuasion",
                    priority = 0,
                    text = "{=*}How about..? (start seduce)",
                    conditionD = new condition { method = "conversation_can_make_pregnant_with_bastard_on_condition" },
                    consequenceD = new consequence { method = "conversation_unblock_all_persuasion_task" },
                });

            AllDialogs.lines.Add(
               new Line
               {
                   type = "npc_line",
                   id = "persuade_to_make_bastard_finished",
                   input = "persuade_to_make_bastard_finished",
                   output = "to_make_bastard_persuasion_caring",
                   priority = 0,
                   text = "{=*}But you still like me right?[ib:nervous]",
                   conditionD = new condition { method = "conversation_make_pregnant_with_bastard_finished_on_condition" },
                   consequenceD = new consequence { method = "leave_encounter_on_consequence" },
               });

            AllDialogs.lines.Add(
               new Line
               {
                   type = "player_line",
                   id = "to_make_bastard_persuasion_caring_0",
                   input = "to_make_bastard_persuasion_caring",
                   output = "close_window",
                   priority = 1,
                   text = "{=*}Yes.",
                   consequenceD = new consequence { method = "change_relation_on_consequence", parameters = new string[] { "5" } },
               });
            AllDialogs.lines.Add(
               new Line
               {
                   type = "player_line",
                   id = "to_make_bastard_persuasion_caring_1",
                   input = "to_make_bastard_persuasion_caring",
                   output = "close_window",
                   priority = 0,
                   text = "{=*}No.",
                   consequenceD = new consequence { method = "change_relation_on_consequence", parameters = new string[] { "-5" } },
                   
               });

            AllDialogs.dialogs.Add(new Dialog
            {
                id = "start_persuade_to_make_bastard_persuasion",
                input = "start_persuade_to_make_bastard_persuasion",
                priority = 125,

                Tasks =
                { 
                    new persuasionTask
                    {
                        id = "start_persuade_to_make_bastard_persuasion_task_0",
                         spokenLine = "spokenLine",
                         finalFailLine = "{=*}finalFailLine 1",
                         tryLaterLine = "{=*}tryLaterLine 1",
                         Options =
                         {
                            new persuasionOption
                            {
                                line = "{=*}Can we drink some tea?",
                                defaultSkills = "Charm",
                                defaultTraits = "Calculating",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = false,
                                traitCorrelations = new traitCorrelations { valor = 0,  mercy = 0,  honor = 1,  generosity = 0,  calculating = 1 },
                                canBlockOtherOption = false,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Yes, lets drink some"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "You are just in time![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, I dont like tea[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "No, you I have no time for your tea![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            },
                            new persuasionOption
                            {
                                line = "{=*}I like how you look from behind.",
                                defaultSkills = "Leadership",
                                defaultTraits = "Valor",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = false,
                                traitCorrelations = new traitCorrelations { valor = 1,  mercy = -1,  honor = -1,  generosity = 0,  calculating = -1 },
                                canBlockOtherOption = false,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                 optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Thanks."},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Thanks, I like too![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, you better dont look to my back[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "Are you kidding?[ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                 }
                            },
                            new persuasionOption
                            {
                                line = "{=*}Let me show my bruises, I got them in last battle.",
                                defaultSkills = "Leadership",
                                defaultTraits = "Mercy",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 1,  mercy = 1,  honor = 0,  generosity = 1,  calculating = 0 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Ok."},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Ok, but I will show mine too![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, not interested.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "I do not care![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                 }
                            },
                            new persuasionOption
                            {
                                line = "{=*}I think I have seen you somewhere, oh yes I saw you in my dreams last night!",
                                defaultSkills = "Charm",
                                defaultTraits = "Honor",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "VeryHard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 1,  mercy = 2,  honor = 0,  generosity = 0,  calculating = 0 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                  optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Ok."},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Ok, but I saw you too![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "Dream again...[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "I do not care about your dreams![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                 }
                            },
                            new persuasionOption
                            {
                                line = "{=*}Ok so here I am, at least one of your wishes has finally come true!",
                                defaultSkills = "Charm",
                                defaultTraits = "Mercy",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "VeryHard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 0,  mercy = 2,  honor = 0,  generosity = 2,  calculating = 0 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Realy? You think like that?[ib:happy]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Ohh, thank you, I'm pleased![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, I've no wishes.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "Go away![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            }
                         },
                        immediateFailLine = "immediateFailLine",
                        reservationType = 0
                    },
                
                    new persuasionTask
                    {
                        id = "start_persuade_to_make_bastard_persuasion_task_1",
                         spokenLine = "spokenLine",
                         finalFailLine = "{=*}No... No. It does not make sense.",
                         tryLaterLine = "{=*}Maybe next time.",
                         Options =
                         {
                            new persuasionOption
                            {
                                line = "{=*}Do you like melons? I swear I see at least two here!",
                                defaultSkills = "Charm",
                                defaultTraits = "Calculating",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = false,
                                traitCorrelations = new traitCorrelations { valor = -2,  mercy = -2,  honor = -1,  generosity = 0,  calculating = -1 },
                                canBlockOtherOption = false,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Not sure, but I like melons.[ib:happy]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "I like them very much. Lets find out where they are?[ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "I do not have such things.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "What melons?! Are you blind?[ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            },
                            new persuasionOption
                            {
                                line = "{=*}Come to my place I will show my horse.",
                                defaultSkills = "Leadership",
                                defaultTraits = "Valor",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = false,
                                traitCorrelations = new traitCorrelations { valor = 2,  mercy = -1,  honor = -2,  generosity = 0,  calculating = 0 },
                                canBlockOtherOption = false,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Sure, I never saw your horse.[ib:happy]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Definitely! Will you let me ride it?[ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "I see it from here. Not so interesting.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "I don't want, I do not care about your horse.[ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            },
                            new persuasionOption
                            {
                                line = "{=*}Let me show my {?PLAYER.GENDER}donut i baked today{?}huge sword{\\?}!",
                                defaultSkills = "Leadership",
                                defaultTraits = "Honor",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "Hard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 1,  mercy = 1,  honor = -2,  generosity = 1,  calculating = -2 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                 optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Sure, I never saw your {?PLAYER.GENDER}donut{?}huge sword{\\?}.[ib:happy]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Definitely! Will you let me {?PLAYER.GENDER}taste donut{?}hold your huge sword{\\?}?[ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "I see it from here. Not so interesting.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "I don't want, I do not care.[ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            },
                            new persuasionOption
                            {
                                line = "{=*}Let me show my ass!",
                                defaultSkills = "Charm",
                                defaultTraits = "Honor",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "VeryHard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 2,  mercy = 2,  honor = -2,  generosity = -1,  calculating = -2 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                 optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Nice.[ib:happy]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Will you let me touch it?[ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, I do not want to see it.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "Don't! What is wrong with you?.[ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            },
                            new persuasionOption
                            {
                                line = "{=*}You are such an angel! We must talk about it now!",
                                defaultSkills = "Charm",
                                defaultTraits = "Mercy",
                                traitEffect = "Positive",
                                persuasionArgumentStrength = "VeryHard",
                                givesCriticalSuccess = true,
                                traitCorrelations = new traitCorrelations { valor = 0,  mercy = -2,  honor = -2,  generosity = -2,  calculating = 0 },
                                canBlockOtherOption = true,
                                canMoveToTheNextReservation = false,
                                isInitiallyBlocked  = false,
                                optionReactions = new reaction[] {
                                    new reaction { persuasionOptionResult = "Success", text = "Realy? You think like that?[ib:nervous]"},
                                    new reaction { persuasionOptionResult = "CriticalSuccess", text = "Ohh, thank you very much![ib:happy]" },
                                    new reaction { persuasionOptionResult = "Failure", text = "No, I'm not.[ib:nervous][ib:closed]" },
                                    new reaction { persuasionOptionResult = "CriticalFailure", text = "Keep your angel thing to your self![ib:closed][if:convo_bared_teeth][if:idle_angry][ib:aggressive]" },
                                }
                            }
                         },
                        immediateFailLine = "immediateFailLine",
                        reservationType = 0
                    },
                },
                lines = {
                    new Line
                    {
                        type = "npc_line",
                        id = "start_persuade_to_make_bastard_persuasion",
                        input = "start_persuade_to_make_bastard_persuasion",
                        output = "persuade_to_make_bastard_reservation",
                        priority = 0,
                        text = "{=*}Uh, wha...{COMMENT}",
                        conditionD = new condition { method = "start_persuade_to_make_bastard_on_condition"},
                        consequenceD = new consequence { method = "persuasion_start", parameters = new string[] {"6 3 1 2 3 0 Hard"}},

                    },
                    new Line
                    {
                        type = "npc_line",
                        id = "persuade_to_make_bastard_rejected",
                        input = "persuade_to_make_bastard_reservation",
                        output = "persuade_to_make_bastard_failed",
                        priority = 0,
                        text = "{=!}{FAILED_PERSUASION_LINE}",
                        conditionD = new condition { method = "persuade_to_make_bastard_failed_on_condition"},
                        consequenceD = new consequence { method = "end_persuasion_on_consequence"},
                    },
                    new Line
                    {
                        type = "npc_line",
                        id = "persuade_to_make_bastard_reservation",
                        input = "persuade_to_make_bastard_reservation",
                        output = "persuade_to_make_bastard_select_option",
                        priority = 0,
                        text = "{=*}So... What is it?",
                        conditionD = new condition { method = "persuade_to_make_bastard_not_failed_on_condition"},
                    },
                    new Line
                    {
                        type = "player_line",
                        id = "persuade_to_make_bastard_select_option_0",
                        input = "persuade_to_make_bastard_select_option",
                        output = "persuade_to_make_bastard_select_option_response",
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
                        id = "persuade_to_make_bastard_select_option_1",
                        input = "persuade_to_make_bastard_select_option",
                        output = "persuade_to_make_bastard_select_option_response",
                        priority = 0,
                        text = "{=!}{PERSUADE_TEXT_1}",
                        conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"1"}},
                        consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"1"} },
                        cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"1"}},
                        pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"1"}},
                    },
                     new Line
                    {
                        type = "player_line",
                        id = "persuade_to_make_bastard_select_option_2",
                        input = "persuade_to_make_bastard_select_option",
                        output = "persuade_to_make_bastard_select_option_response",
                        priority = 0,
                        text = "{=!}{PERSUADE_TEXT_2}",
                        conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"2"}},
                        consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"2"} },
                        cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"2"}},
                        pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"2"}},
                    },
                    new Line
                    {
                        type = "player_line",
                        id = "persuade_to_make_bastard_select_option_3",
                        input = "persuade_to_make_bastard_select_option",
                        output = "persuade_to_make_bastard_select_option_response",
                        priority = 0,
                        text = "{=!}{PERSUADE_TEXT_3}",
                        conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"3"}},
                        consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"3"} },
                        cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"3"}},
                        pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"3"}},
                    },
                    new Line
                    {
                        type = "player_line",
                        id = "persuade_to_make_bastard_select_option_4",
                        input = "persuade_to_make_bastard_select_option",
                        output = "persuade_to_make_bastard_select_option_response",
                        priority = 0,
                        text = "{=!}{PERSUADE_TEXT_4}",
                        conditionD = new condition { method = "persuasion_select_option_on_condition", parameters = new string[] {"4"}},
                        consequenceD = new consequence { method = "persuasion_block_select_option_on_consequence", parameters = new string[] {"4"} },
                        cConditionD = new clickableCondition { method = "persuasion_clickable_option_on_condition", parameters = new string[] {"4"}},
                        pOptionD = new persuasion { method = "persuasion_setup_option", parameters = new string[] {"4"}},
                    },
                    new Line
                    {
                        type = "npc_line",
                        id = "persuade_to_make_bastard_select_option_response",
                        input = "persuade_to_make_bastard_select_option_response",
                        output = "persuade_to_make_bastard_reservation",
                        priority = 0,
                        text = "{=*}{PERSUASION_REACTION}",
                        conditionD = new condition { method = "persuasion_selected_option_response_on_condition"},
                        consequenceD = new consequence { method = "persuasion_selected_option_response_on_consequence"},
                    },
                    new Line
                    {
                        type = "npc_line",
                        id = "persuade_to_make_bastard_failed",
                        input = "persuade_to_make_bastard_failed",
                        output = "close_window",
                        priority = 0,
                        text = "{=*}Go away {?PLAYER.GENDER}slut{?}pervert{\\?}!",
                        consequenceD = new consequence { method = "persuade_to_make_bastard_failed_on_consequence"},
                    },
                    new Line
                    {
                        type = "npc_line",
                        id = "persuade_to_make_bastard_success",
                        input = "persuade_to_make_bastard_reservation",
                        output = "persuade_to_make_bastard_finished",
                        priority = 0,
                        text = "{=*}Ohh...",
                        conditionD = new condition { method = "persuasion_success_on_condition"},
                        consequenceD = new consequence { method = "conversation_make_pregnant_with_bastard_on_consequence"},
                    }
                }
            });
           
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

        private void conversation_unblock_persuasion_task(string reservationType)
        {
            PersuasionTask currentPersuasionTask = _allReservations[int.Parse(reservationType)];
            currentPersuasionTask.UnblockAllOptions();
        }

        private void conversation_unblock_all_persuasion_task()
        {
            foreach (PersuasionTask persuasionTask in this._allReservations)
            {
                persuasionTask.UnblockAllOptions();  
            }
        }

        private void conversation_unblock_current_persuasion_task()
        {
            PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
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

        private bool persuade_to_make_bastard_failed_on_condition()
        {
            PersuasionTask currentPersuasionTask = GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
            {
                MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", currentPersuasionTask.FinalFailLine, false);
                return true;
            }
            return false;
        }

        private bool persuade_to_make_bastard_not_failed_on_condition()
        {
            return !persuade_to_make_bastard_failed_on_condition() && !persuasion_success_on_condition();
        }

        private bool conversation_can_make_pregnant_with_bastard_on_condition()
        {
            BastardPregnancy pregnancy = GetCampaignBehavior<BastardPregnancy>();
            Hero mother = Hero.MainHero.IsFemale ? Hero.MainHero : Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            Hero father = !Hero.MainHero.IsFemale ? Hero.MainHero : !Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            if (mother == null || father == null)
                return false;
            bool can = pregnancy.CanMakePregnantWithBastard(mother, father);
            return can;
        }

        private bool conversation_make_pregnant_with_bastard_finished_on_condition()
        {
            Hero mother = Hero.MainHero.IsFemale ? Hero.MainHero : Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            Hero father = !Hero.MainHero.IsFemale ? Hero.MainHero : !Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            if (mother != null && father != null && !Hero.MainHero.IsFemale)
            {
                return mother.IsPregnant;
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool start_persuade_to_make_bastard_on_condition()
        {
            Hero mother = Hero.MainHero.IsFemale ? Hero.MainHero : Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            Hero father = !Hero.MainHero.IsFemale ? Hero.MainHero : !Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            if (mother != null && father != null)
            {
                BastardPregnancy pregnancy = GetCampaignBehavior<BastardPregnancy>();
                MBTextManager.SetTextVariable("COMMENT", "What is going on?", false);
            }
            else
            {
                return false;
            }
            return true;
        }

        private void conversation_make_pregnant_with_bastard_on_consequence()
        {
            Hero mother = Hero.MainHero.IsFemale ? Hero.MainHero : Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            Hero father = !Hero.MainHero.IsFemale ? Hero.MainHero : !Hero.OneToOneConversationHero.IsFemale ? Hero.OneToOneConversationHero : null;
            if (mother != null && father != null)
            {
                BastardPregnancy pregnancy = GetCampaignBehavior<BastardPregnancy>();
                if (pregnancy.CanMakePregnantWithBastard(mother, father))
                {
                    pregnancy.MakePregnantWithBastard(mother, father);
                    Hero.MainHero.SetTraitLevel(DefaultTraits.Honor, Hero.MainHero.GetTraitLevel(DefaultTraits.Honor) - 1);
                }
            }
        }

        private void leave_encounter_on_consequence()
        {
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
        }

        private void end_persuasion_on_consequence()
        {
            ConversationManager.EndPersuasion();
        }

        private bool persuasion_success_on_condition()
        {
            return ConversationManager.GetPersuasionProgressSatisfied();
        }

        private void persuade_to_make_bastard_failed_on_consequence()
        {
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, -10, false, true);
        }
        private void change_relation_on_consequence(string value)
        {
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, int.Parse(value), false, true);
        }
        #endregion

    }
}
