using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Database.Engine;
using FractalPlatform.Database.Engine.Info;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FractalPlatform.BowlingScores {
    public class BowlingScoresApplication: BaseApplication {
        private void Dashboard() => FirstDocOf("Dashboard").OpenForm();
        
        public override void OnStart() {
            var password = "123";

            if (Context.UrlTag == password) {
                Dashboard();
            } else {
                InputBox("Password", "Enter password", result => {
                    if (result.Result) {
                        var firstValue = result.FindFirstValue("Password");
                        if (firstValue == password) {
                            Context.UrlTag = firstValue;
                            Dashboard();
                        } else {
                            MessageBox("Wrong credentials.");
                        }
                    }
                });
            }
        }

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
            var juliaScores = DocsOf("Scores")
                                .SelectValues("{'OnDate':$,'Julia':{@Type:$}}", type)
                                .Select(vals => new
                                {
                                    Name = "Julia",
                                    Value = double.Parse(vals[1]),
                                    Group = DateTime.Parse(vals[0], CultureInfo.InvariantCulture).ToString("yyyyMMdd")
                                })
                                .ToList();

            var viacheslavScores = DocsOf("Scores")
                                    .SelectValues("{'OnDate':$,'Viacheslav':{@Type:$}}", type)
                                    .Select(vals => new
                                    {
                                        Name = "Viacheslav",
                                        Value = double.Parse(vals[1]),
                                        Group = DateTime.Parse(vals[0], CultureInfo.InvariantCulture).ToString("yyyyMMdd")
                                    })
                                    .ToList();

            var chartData = new
            {
                Control = new
                {
                    Title = new { Name = type, X = "OnDate", Y = type },
                    Columns = juliaScores.Union(viacheslavScores).ToList()
                }
            };
            
            chartData.ToCollection("Bowling " + type)
                     .SetUIDimension("{'ReadOnly':true,'Control':{'ControlType':'Chart','Style':'Type:Bar'}}")
                     .OpenForm();
        }
    
        public override BaseRenderForm CreateRenderForm(DOMForm form) => new ExtendedRenderForm((BaseApplication) this, form);
    }
}