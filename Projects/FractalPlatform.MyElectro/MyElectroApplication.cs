using System;
using System.Collections.Generic;
using System.Linq;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Common.Enums;
using FractalPlatform.Database.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FractalPlatform.MyElectro
{
    public class MyElectroApplication : BaseApplication
    {
        private object GetSchedule(string schedule, bool isTomorrow)
        {
            string prevElectricity = null;
            int groupId = 0;
            return schedule.ToCollection()
                   .ToAttrList()
                   .Select(x =>
                   {
                       var hour = int.Parse(x.Key.FirstPath);
                       var startHour = hour - 1;
                       var endHour = hour;
                       var strElectricity = x.Value.ToString();
                       
					   if (strElectricity == "first")
                       {
                           var result = new List<dynamic>();

                           var electricity = "ВІДСУТНЄ";

						   if (prevElectricity == null || prevElectricity != electricity)
						   {
							   groupId++;
							   prevElectricity = electricity;
						   }

						   result.Add(new
						   {
							   StartHour = startHour,
							   StartMinute = 0,
							   EndHour = startHour,
							   EndMinute = 30,
							   Electricity = electricity,
							   GroupId = groupId
						   });

						   electricity = "ПРИСУТНЄ";

						   if (prevElectricity == null || prevElectricity != electricity)
						   {
							   groupId++;
							   prevElectricity = electricity;
						   }

						   result.Add(new
						   {
							   StartHour = startHour,
							   StartMinute = 30,
							   EndHour = endHour,
							   EndMinute = 0,
							   Electricity = electricity,
							   GroupId = groupId
						   });

                           return result;
                       }
                       else if (strElectricity == "second")
                       {
						   var result = new List<dynamic>();

						   var electricity = "ПРИСУТНЄ";

						   if (prevElectricity == null || prevElectricity != electricity)
						   {
							   groupId++;
							   prevElectricity = electricity;
						   }

						   result.Add(new
						   {
							   StartHour = startHour,
							   StartMinute = 0,
							   EndHour = startHour,
							   EndMinute = 30,
							   Electricity = electricity,
							   GroupId = groupId
						   });

						   electricity = "ВІДСУТНЄ";

						   if (prevElectricity == null || prevElectricity != electricity)
						   {
							   groupId++;
							   prevElectricity = electricity;
						   }

						   result.Add(new
						   {
							   StartHour = startHour,
							   StartMinute = 30,
							   EndHour = endHour,
							   EndMinute = 0,
							   Electricity = electricity,
							   GroupId = groupId
						   });

						   return result;
					   }
					   else
                       {
						   var electricity = strElectricity
											.Replace("yes", $"ПРИСУТНЄ")
											.Replace("no", $"ВІДСУТНЄ")
											.Replace("maybe", "НЕВІДОМО");

						   if (prevElectricity == null || prevElectricity != electricity)
						   {
							   groupId++;
							   prevElectricity = electricity;
						   }

						   return new List<dynamic>
						   {
								new
								{
									StartHour = startHour,
									StartMinute = 0,
									EndHour = endHour,
									EndMinute = 0,
									Electricity = electricity,
									GroupId = groupId
								}
						   };
					   }
				   })
                  .SelectMany(x => x)
                  .GroupBy(x => x.GroupId,
                  (k, g) => new
                  {
                      StartHour = g.Min(x => x.StartHour),
                      StartMinute = g.First().StartMinute,
                      EndHour = g.Max(x => x.EndHour),
                      EndMinute = g.Last().EndMinute,
                      Electricity = g.First().Electricity
                  })
                  .Where(x => (DateTime.Now.Hour < x.EndHour ||
                               (DateTime.Now.Hour == x.EndHour &&
                                DateTime.Now.Minute < x.EndMinute) ||
                                isTomorrow))
                  .Select(x =>
                  {
                      var start = new TimeSpan(x.StartHour, x.StartMinute, 0);
                      var end = new TimeSpan(x.EndHour, x.EndMinute, 0);
                      var total = end - start;
                      var sign = x.Electricity == "ПРИСУТНЄ" ? "+" : "-";
                      var isUndefined = x.StartHour == 0 &&
                                        x.StartMinute == 0 &&
                                        x.EndHour == 24 &&
                                        x.EndMinute == 0;
                      
                      return new
                      {
                        Час = $"{x.StartHour:00}:{x.StartMinute:00} - {x.EndHour:00}:{x.EndMinute:00}",
                        Світло = !isUndefined || !isTomorrow ? x.Electricity : "НЕВІДОМО",
                        Годин = !isUndefined ? $"{sign} {(int)total.TotalHours:00}:{total.Minutes:00}" : null
                      };
                  }).ToList();
        }
        
        private string DownloadData(bool isTomorrow)
        {
            var html = REST.Get("https://www.dtek-kem.com.ua/ua/shutdowns");
            var startIndex = html.IndexOf("DisconSchedule.fact =") + 22;
            var endIndex = html.IndexOf("</script>", startIndex);
            var json = html.Substring(startIndex, endIndex - startIndex);
            var data = (JObject)JsonConvert.DeserializeObject(json);
            string dtekGroupId = "GPV1.1";
            
            if(Context.HasUrlTag)
            {
                dtekGroupId = Context.UrlTag;
            }

            var entries = data["data"].Children<JProperty>().ToList();
            return (!isTomorrow ? entries[0].Value[dtekGroupId] : entries[1].Value[dtekGroupId]).ToString();
        }

        public override void OnStart()
        {
            //1. Download data
            var period = TimeSpan.FromMinutes(15);
            var today = UseCache(() => DownloadData(false), period, "Today");
            var tomorrow = UseCache(() => DownloadData(true), period, "Tomorrow");

            //2. Show data
            new
            {
                Label = "ДТЕК прогнозує наявність світла:",
                Сьогодні = GetSchedule(today, false),
                Завтра = GetSchedule(tomorrow, true)
            }
            .ToCollection("Моніторинг")
            .SetUIDimension("{'Style':'Hide:Number;Cancel:Refresh','ReadOnly':true,'Label':{'ControlType':'Label'}}")
            .SetDimension(DimensionType.Theme, "{'DefaultTheme': 'LightBlue', 'ChooseThemeOnLoginPage': false, 'ChooseThemeOnAllPages': false}")
            .OpenForm();
        }
    }
}