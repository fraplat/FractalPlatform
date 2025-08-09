using System.Linq;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Common.Enums;

namespace FractalPlatform.LearnDictionary
{
    public class LearnDictionaryApplication : BaseApplication
    {
        public override void OnStart()
        {
            FirstDocOf("Dashboard")
                .OpenForm();      
        }
        
        public override bool OnEventDimension(EventInfo info)
        {
            var path = info.AttrPath.ToString();

            switch(path)
            {
                case @"Learn":
                {
                    ModifyDocsWhere("Words","{'Words':[{'IsLearned':false}]}")
                        .OpenForm("{'Words':[{'Word':$,'Trans':$,'IsLearned':$}]}");

                    break;
                }
                case @"NewWord":
                {
                    CreateNewDocForArray("NewWord", "Words", "{'Words':[$]}")
                        .OpenForm();

                    break;
                }
                case @"Reset":
                {
                    MessageBox("Are you sure to reset learned words?",
                               "Reset",
                               MessageBoxButtonType.YesNo,
                               result =>
                    {
                        if (result.Result)
                        {
                            ModifyFirstDocOf("Words")
                                .Update("{'Words':[{'IsLearned':false}]}");
                        }
                    });
                    
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

            switch(info.Variable)
            {
                case @"LearnedWordsVariable":
                {
                    result = DocsWhere("Words","{'Words':[{'IsLearned':true}]}")
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
    }
}