using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Database.Engine;
using FractalPlatform.Database.Engine.Info;
using System;
using System.Globalization;
using System.Linq;

namespace FractalPlatform.BowlingScores {
    public class BowlingScoresApplication: BaseApplication {

        public override void OnStart() => FirstDocOf("Dashboard").OpenForm();

        public override bool OnEventDimension(EventInfo info) {
            switch (info.AttrPath.ToString()) {
                case @"Score\NewScore":
                    FirstDocOf("NewScore").OpenForm(result => {
                        if (result.Result) {
                            var data = new {
                                OnDate = DateTime.Now,
                                    Image = result.FindFirstValue("Image"),
                                    Viacheslav = new {
                                        Points = result.IntValue("{'Scores':{'Viacheslav':{'Points':$}}}"),
                                            Strikes = result.IntValue("{'Scores':{'Viacheslav':{'Strikes':$}}}")
                                    },
                                    Julia = new {
                                        Points = result.IntValue("{'Scores':{'Julia':{'Points':$}}}"),
                                            Strikes = result.IntValue("{'Scores':{'Julia':{'Strikes':$}}}")
                                    }
                            };

                            this.AddDoc("Scores", data);
                        }
                    });
                    break;
                case @"Score\Scores":
                    ModifyDocsOf("Scores").OpenForm();
                    break;
                case @"Report\Points":
                    Report("Points");
                    break;
                case @"Report\Strikes":
                    Report("Strikes");
                    break;
                default:
                    return base.OnEventDimension(info);
            }
            return true;
        }
        
        private void Report(string type)
        {
            var janeScores = DocsOf("Scores")
                                .SelectValues("{'OnDate':$,'Jane':{@Type:$}}", type)
                                .Select(vals => new
                                {
                                    Name = "Jane",
                                    Value = double.Parse(vals[1]),
                                    Group = DateTime.Parse(vals[0], CultureInfo.InvariantCulture).ToString("yyyyMMdd")
                                })
                                .ToList();

            var bobScores = DocsOf("Scores")
                                    .SelectValues("{'OnDate':$,'Bob':{@Type:$}}", type)
                                    .Select(vals => new
                                    {
                                        Name = "Bob",
                                        Value = double.Parse(vals[1]),
                                        Group = DateTime.Parse(vals[0], CultureInfo.InvariantCulture).ToString("yyyyMMdd")
                                    })
                                    .ToList();

            var chartData = new
            {
                Control = new
                {
                    Title = new { Name = type, X = "OnDate", Y = type },
                    Columns = janeScores.Union(bobScores).ToList()
                }
            };
            
            chartData.ToCollection("Bowling " + type)
                     .SetUIDimension("{'ReadOnly':true,'Control':{'ControlType':'Chart','Style':'Type:Bar'}}")
                     .OpenForm();
        }
    
        public override BaseRenderForm CreateRenderForm(DOMForm form) => new ExtendedRenderForm(this, form);
    }
}