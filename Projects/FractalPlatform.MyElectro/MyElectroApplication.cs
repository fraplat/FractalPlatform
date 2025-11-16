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
                  .Where(x => DateTime.Now.Hour < x.EndHour || isTomorrow)
                  .Select(x => new
                  {
                      Час = $"{x.StartHour.ToString("00")}:{x.StartMinute.ToString("00")} - {x.EndHour.ToString("00")}:{x.EndMinute.ToString("00")}",
                      Світло = x.Electricity
                  }).ToList();
        }
        
        private object GetSchedule2(string schedule, bool isTomorrow)
        {
            string prevElectricity = null;
            int groupId = 0;
            
            return schedule
                   .ToCollection()
                   .ToAttrList()
                   .Select(x =>
                   {
                       var hour = int.Parse(x.Key.FirstPath);
                       var startHour = hour - 1;
                       var endHour = hour;
                       var strElectricity = x.Value.ToString();
                       var electricity = strElectricity
                                        .Replace("yes", $"ПРИСУТНЄ")
                                        .Replace("no", $"ВІДСУТНЄ")
                                        .Replace("maybe", "НЕВІДОМО")
                                        .Replace("first", $"ВІДСУТНЄ")
                                        .Replace("second", $"ВІДСУТНЄ");

                       if (prevElectricity == null || prevElectricity != electricity)
                       {
                           groupId++;
                           prevElectricity = electricity;
                       }

                       return new
                       {
                           StartHour = startHour,
                           EndHour = endHour,
                           Electricity = electricity,
                           GroupId = groupId,
                           IsFirst = strElectricity == "first",
                           IsSecond = strElectricity == "second"
                       };
                   })
                  .GroupBy(x => x.GroupId,
                  (k, g) => new
                  {
                      StartHour = g.Min(x => x.StartHour),
                      StartMinutes = g.Any(x => x.IsFirst) ? 30 : 0,
                      EndHour = g.Max(x => x.EndHour),
                      EndMinutes = g.Any(x => x.IsSecond) ? 0 : 30,
                      Electricity = g.First().Electricity
                  })
                  .Where(x => DateTime.Now.Hour < x.EndHour || isTomorrow)
                  .Select(x => new
                  {
                      Час = $"{x.StartHour.ToString("00")}:{x.StartMinutes.ToString("00")} - {x.EndHour.ToString("00")}:{x.EndMinutes.ToString("00")}",
                      Світло = x.Electricity
                  }).ToList();
        }

        private string DownloadData(bool isTomorrow)
        {
            var html = REST.Get("https://www.dtek-kem.com.ua/ua/shutdowns");
            var startIndex = html.IndexOf("DisconSchedule.fact =") + 22;
            var endIndex = html.IndexOf("</script>", startIndex);
            var json = html.Substring(startIndex, endIndex - startIndex);
            var data = (JObject)JsonConvert.DeserializeObject(json);
            var dtekGroupId = "GPV1.1";

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