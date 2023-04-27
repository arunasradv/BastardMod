using BastardsMod.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BastardsMod
{
    public class DialogHelper : DialogHelperBase
    {
        public override void Save(string file_name)
        {
            base.Save(file_name);
        }

        public override void Load(string file_name)
        {
           base.Load(file_name);
        }

        public List<PersuasionTask> GetAllDialogTasks()
        {
            List<PersuasionTask> pTasks = new List<PersuasionTask>();
            foreach (Dialog D in dialogs)
            {
                pTasks.AddRange(AddDialogTasks(D.Tasks));
            }
            return pTasks;
        }

        public List<PersuasionTask> AddDialogTasks(List<persuasionTask> tasks)
        {
            List<PersuasionTask> ptasks = new List<PersuasionTask>();

            foreach (persuasionTask pT in tasks)
            {
                PersuasionTask persuasionTask = new PersuasionTask(pT.reservationType);
                persuasionTask.FinalFailLine = new TextObject(pT.finalFailLine, null);
                persuasionTask.TryLaterLine = new TextObject(pT.tryLaterLine, null);
                persuasionTask.SpokenLine = new TextObject(pT.spokenLine, null);

                foreach (persuasionOption pTo in pT.Options)
                {
                    SkillObject skill = (SkillObject)GetPropValue(Game.Current.DefaultSkills, pTo.defaultSkills);
                    TraitObject trait = (TraitObject)GetPropValue(Campaign.Current.DefaultTraits, pTo.defaultTraits);
                    TraitEffect traitEffect = (TraitEffect)Enum.Parse(typeof(TraitEffect), pTo.traitEffect);
                    PersuasionArgumentStrength pStrength = (PersuasionArgumentStrength)Enum.Parse(typeof(PersuasionArgumentStrength), pTo.persuasionArgumentStrength);
                    Tuple<TraitObject, int>[] traitCorrelations = pTo.traitCorrelations == null ? null : GetTraitCorrelations(
                        pTo.traitCorrelations.valor,
                        pTo.traitCorrelations.mercy,
                        pTo.traitCorrelations.honor,
                        pTo.traitCorrelations.generosity,
                        pTo.traitCorrelations.calculating);

                    PersuasionOptionArgs option = new PersuasionOptionArgs(skill, trait, traitEffect, pStrength, pTo.givesCriticalSuccess,
                    new TextObject(pTo.line, null), traitCorrelations, pTo.canBlockOtherOption, pTo.canMoveToTheNextReservation, pTo.isInitiallyBlocked);
                    persuasionTask.AddOptionToTask(option);
                }

                ptasks.Add(persuasionTask);
            }

            return ptasks;
        }
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        private Tuple<TraitObject, int>[] GetTraitCorrelations(int valor = 0, int mercy = 0, int honor = 0, int generosity = 0, int calculating = 0)
        {
            return new Tuple<TraitObject, int>[]
            {
                new Tuple<TraitObject, int>(DefaultTraits.Valor, valor),
                new Tuple<TraitObject, int>(DefaultTraits.Mercy, mercy),
                new Tuple<TraitObject, int>(DefaultTraits.Honor, honor),
                new Tuple<TraitObject, int>(DefaultTraits.Generosity, generosity),
                new Tuple<TraitObject, int>(DefaultTraits.Calculating, calculating)
            };
        }

        public List<DialogFlow> GetDialogFlows(object invokerClass)
        {
            List < DialogFlow > dialog_flow_list = new List < DialogFlow >();

            foreach (Dialog D in dialogs)
            {
                DialogFlow dialogFlow = DialogFlow.CreateDialogFlow(D.input, D.priority);

                foreach (Line L in D.lines)
                {
                    ConversationSentence cs = null;

                    if (L.type == "player_line")
                    {
                        cs = dialogFlow.AddPlayerLine(L.id, L.input, L.output, L.text,
                               L.conditionD == null ? null : new ConversationSentence.OnConditionDelegate(() =>
                               {
                                   return Convert.ToBoolean(Common.UseMethod(invokerClass, L.conditionD.method, L.conditionD.parameters));
                               }),
                               L.consequenceD == null ? null : new ConversationSentence.OnConsequenceDelegate(() =>
                               {
                                   Common.UseMethod(invokerClass, L.consequenceD.method, L.consequenceD.parameters);
                               }),
                               invokerClass,
                               Convert.ToInt32(L.priority),
                               L.cConditionD == null ? null : new ConversationSentence.OnClickableConditionDelegate((out TextObject hintText) =>
                               {
                                   return Convert.ToBoolean(Common.UseMethodWithOut(invokerClass, L.cConditionD.method, out hintText, L.cConditionD.parameters));
                               }),
                               L.pOptionD == null ? null : new ConversationSentence.OnPersuasionOptionDelegate(() =>
                               {
                                   return ((PersuasionOptionArgs)Common.UseMethod(invokerClass, L.pOptionD.method, L.pOptionD.parameters));
                               }));

                    }
                    else if (L.type == "npc_line")
                    {
                        cs = dialogFlow.AddDialogLine(L.id, L.input, L.output, L.text,
                               L.conditionD == null ? null : new ConversationSentence.OnConditionDelegate(() =>
                               {
                                   return Convert.ToBoolean(Common.UseMethod(invokerClass, L.conditionD.method, L.conditionD.parameters));
                               }),
                               L.consequenceD == null ? null : new ConversationSentence.OnConsequenceDelegate(() =>
                               {
                                   Common.UseMethod(invokerClass, L.consequenceD.method, L.consequenceD.parameters);
                               }),
                               invokerClass,
                               Convert.ToInt32(L.priority),
                                L.cConditionD == null ? null : new ConversationSentence.OnClickableConditionDelegate((out TextObject hintText) =>
                                {
                                    return Convert.ToBoolean(Common.UseMethodWithOut(invokerClass, L.cConditionD.method, out hintText, L.cConditionD.parameters));
                                }));
                    }

                    if (cs != null)
                    {
                        if (L.Variations != null)
                        {
                            foreach (variations v in L.Variations)
                            {
                                List<object> list = new List<object>();
                                list.Add(v.text);
                                foreach (var item in v.tag)
                                {
                                    list.Add(item.tag_name);
                                    list.Add(item.weight);
                                }

                                cs = cs.Variation(list.ToArray());
                            }
                        }
                    }
                }
                dialog_flow_list.Add(dialogFlow);                
            }
            return dialog_flow_list;
        }

        public void LoadDialogs(CampaignGameStarter starter, object invokerClass)
        {
            foreach (Line L in lines)
            {
                ConversationSentence cs = null;

                if (L.type == "player_line")
                {
                    cs = starter.AddPlayerLine(L.id, L.input, L.output, L.text,
                           L.conditionD == null ? null : new ConversationSentence.OnConditionDelegate(() =>
                           {
                               return Convert.ToBoolean(Common.UseMethod(invokerClass, L.conditionD.method, L.conditionD.parameters));
                           }),
                           L.consequenceD == null ? null : new ConversationSentence.OnConsequenceDelegate(() =>
                           {
                               Common.UseMethod(invokerClass, L.consequenceD.method, L.consequenceD.parameters);
                           }),
                           Convert.ToInt32(L.priority),
                           L.cConditionD == null ? null : new ConversationSentence.OnClickableConditionDelegate((out TextObject hintText) =>
                           {
                               return Convert.ToBoolean(Common.UseMethodWithOut(invokerClass, L.cConditionD.method, out hintText));
                           }),
                           L.pOptionD == null ? null : new ConversationSentence.OnPersuasionOptionDelegate(() =>
                           {
                               return ((PersuasionOptionArgs)Common.UseMethod(invokerClass, L.pOptionD.method, L.pOptionD.parameters));
                           }));

                }
                else if (L.type == "npc_line")
                {
                    cs = starter.AddDialogLine(L.id, L.input, L.output, L.text,
                           L.conditionD == null ? null : new ConversationSentence.OnConditionDelegate(() =>
                           {
                               return Convert.ToBoolean(Common.UseMethod(invokerClass, L.conditionD.method, L.conditionD.parameters));
                           }),
                           L.consequenceD == null ? null : new ConversationSentence.OnConsequenceDelegate(() =>
                           {
                               Common.UseMethod(invokerClass, L.consequenceD.method, L.consequenceD.parameters);
                           }),
                           Convert.ToInt32(L.priority),
                            L.cConditionD == null ? null : new ConversationSentence.OnClickableConditionDelegate((out TextObject hintText) =>
                            {
                                return Convert.ToBoolean(Common.UseMethodWithOut(invokerClass, L.cConditionD.method, out hintText));
                            }));
                }

                if (cs != null)
                {
                    if (L.Variations != null)
                    {
                        foreach (variations v in L.Variations)
                        {
                            List<object> list = new List<object>();
                            list.Add(v.text);
                            foreach (var item in v.tag)
                            {
                                list.Add(item.tag_name);
                                list.Add(item.weight);
                            }

                            cs = cs.Variation(list.ToArray());
                        }
                    }
                }
            }
        }
    }
}
