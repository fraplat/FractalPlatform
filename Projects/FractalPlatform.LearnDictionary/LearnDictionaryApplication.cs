using System.Linq;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;

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
                    //handle NewWord button click

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