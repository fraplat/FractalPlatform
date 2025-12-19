using System.Linq;
using System;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Client.App;
using FractalPlatform.Common.Enums;
using FractalPlatform.Common.Clients;

namespace FractalPlatform.LearnDictionary
{
    public class LearnDictionaryApplication : BaseApplication
    {
        public override void OnStart() =>
            UsePassword("777", () => FirstDocOf("Dashboard").OpenForm());

        public override bool OnEventDimension(EventInfo info)
        {
            var path = info.AttrPath.LastPath;

            switch (path)
            {
                case @"Learn":
                    {
                        ModifyDocsWhere("Words", "{'Words':[{'IsLearned':false}]}")
                            .OpenForm("{'Words':[{'Word':$,'Phrase':$,'Trans':$,'IsLearned':$,'LearnText':$}]}");

                        break;
                    }
                case @"NewWord":
                    {
                        CreateNewDocForArray("NewWord", "Words", "{'Words':[$]}")
                            .OpenForm();

                        break;
                    }
                case @"Edit":
                    {
                        ModifyFirstDocOf("Words")
                            .ExtendUIDimension("{'Layout':'','IsRawPage':false,'Style':'Hide:IsLearned,IsArchived,LearnText'}")
                            .ResetDimension(DimensionType.Pagination)
                            .OpenForm();

                        break;
                    }
                case @"LearnText":
                    {
                        var word = DocsWhere("Words", info.AttrPath).Value("{'Words':[{'Word':$}]}");

                        var aboutMe = FirstDocOf("MyLife").Value("{'AboutMe':$}");

                        var answer = AI.Generate($"Create a micro text with 3-4 simple sentences with phrase: {word}. These sentences should be imagined about me and my life: {aboutMe}", AIModel.GPT4oMini);
                        var text = answer.Text;

                        text = text.Replace(word, $"<b>{word}</b>");

                        MessageBox(text, "About my life", MessageBoxButtonType.Ok);

                        break;
                    }
                case @"Reset":
                    {
                        MessageBox("Are you sure you want to reset learned words?",
                                   "Reset",
                                   MessageBoxButtonType.YesNo,
                                   result =>
                        {
                            if (result.Result)
                            {
                                ModifyDocsWhere("Words", "{'Words':[{'IsLearned':true,'IsArchived':false}]}")
                                    .Update("{'Words':[{'IsLearned':false}]}");
                            }
                        });

                        break;
                    }
                case @"MyLife":
                    {
                        ModifyFirstDocOf("MyLife")
                            .OpenForm();

                        break;
                    }
                case @"Articles":
                    {
                        ModifyFirstDocOf("Articles")
                            .OpenForm();

                        break;
                    }
                case @"Archive":
                    {
                        MessageBox("Are you sure you want to archive learned words?",
                                   "Reset",
                                   MessageBoxButtonType.YesNo,
                                   result =>
                        {
                            if (result.Result)
                            {
                                ModifyDocsWhere("Words", "{'Words':[{'IsLearned':true}]}")
                                    .Update("{'Words':[{'IsArchived':true}]}");
                            }
                        });

                        break;
                    }
                case @"Prompt":
                    {
                        var topics = FirstDocOf("Articles")
                                        .GetWhere("{'Articles':[{'IsLearned':false}]}")
                                        .Values("{'Articles':[{'Name':$}]}");

                        if (topics.Any())
                        {
                            var idx = new Random().Next() % topics.Count;

                            var topic = topics[idx];

                            FirstDocOf("Articles")
                                .GetWhere("{'Articles':[{'Name':@Name}]}", topic)
                                .Update("{'Articles':[{'IsLearned':true}]}");

                            new
                            {
                                Prompt = $"Now you are teacher of english. We are talking about low-code platforms and how to sale products. We have new topic to discuss: {topic}. Please repeat our topic then ask questions one by one according this topic."
                            }
                            .ToCollection("Prompt")
                            .SetUIDimension("{'Prompt':{'ControlType':'RichTextBox'},'ReadOnly':true}")
                            .OpenForm();
                        }
                        else
                        {
                            MessageBox("You have no topics to learn");
                        }

                        break;
                    }
                default:
                    {
                        return base.OnEventDimension(info);
                    }
            }

            return true;
        }

        public override object OnComputedDimension(ComputedInfo info)
        {
            object result = null;

            switch (info.Variable)
            {
                case @"LearnedWordsVariable":
                    {
                        result = DocsWhere("Words", "{'Words':[{'IsLearned':true}]}")
                                    .Count("{'Words':[{'Word':$}]}");
                        break;
                    }
                case @"TotalWordsVariable":
                    {
                        result = FirstDocOf("Words")
                                    .Count("{'Words':[{'Word':$}]}");
                        break;
                    }
                default:
                    {
                        return base.OnComputedDimension(info);
                    }
            }

            return result;
        }

        public override bool OnMovePageDimension(MoveInfo info)
        {
            if (info.Collection.Name == "Words")
            {
                SaveForm();
            }

            return true;
        }
    }
}