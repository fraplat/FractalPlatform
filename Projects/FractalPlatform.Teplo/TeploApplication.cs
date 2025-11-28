using System.Collections.Generic;
using System.Linq;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using System;
using System.Globalization;

namespace FractalPlatform.Teplo
{
    public class TeploApplication : BaseApplication
    {
        public class Indicator
        {
            public string Рік {get; set;}
            public string Місяць {get; set;}
            public string Квартира {get; set;}
            public string Показник {get; set;}
        }
        
        public class Accounts
        {
            public List<Indicator> Показники {get; set;}
        }
        
        public override void OnStart() =>
            UsePassword("Chavdar22", () => FirstDocOf("Dashboard")
                                        .ExtendDocument(DQL("{'Рік':@Year,'Місяць':@Month}",
                                                        DateTime.Now.Year,
                                                        DateTime.Now.ToString("MMMM", new CultureInfo("uk-UA"))))
                                        .OpenForm(result =>
                                        {
                                            var year = result.Collection.FindFirstValue("Рік");
                                            var month = result.Collection.FindFirstValue("Місяць");
                                            var apartment = result.Collection.FindFirstValue("Квартира");
                                            var value = result.Collection.FindFirstValue("Показник");
                                            
                                            if(result.Result)
                                            {
                                                var query = ModifyDocsWhere("Accounts",
                                                                     "{'Показники':[{'Рік':@Year,'Місяць':@Month,'Квартира':@Apartment}]}",
                                                                      year, month, apartment);
                                                             
                                                if(query.Any())
                                                {
                                                    query.Update("{'Показники':[{'Показник':@Value}]}", value);
                                                    
                                                    MessageBox("Дякуємо, Ваші показники оновлено !", "Внесення показників");
                                                }
                                                else
                                                {
                                                    ModifyFirstDocOf("Accounts")
                                                        .Update("{'Показники':[Add,{'Рік':@Year,'Місяць':@Month,'Квартира':@Apartment,'Показник':@Value}]}",
                                                                 year, month, apartment, value);
                                                    
                                                    MessageBox("Дякуємо, Ваші показники внесено !", "Внесення показників");
                                                }
                                            }
                                            else
                                            {
                                                FirstDocOf("FilterHistory")
                                                    .OpenForm(result => 
                                                    {
                                                        if(result.Result)
                                                        {
                                                            var year = result.FindFirstValue("Рік");
                                                            var month = result.FindFirstValue("Місяць");
                                                            var apartment = result.FindFirstValue("Квартира");
                                                            
                                                            var accounts = FirstDocOf("Accounts")
                                                                                .SelectOne<Accounts>();
                                                            
                                                            new 
                                                            {
                                                                Показники = accounts.Показники
                                                                                    .Where(x => (year == "ВСІ" || x.Рік == year) &&
                                                                                                 (month == "ВСІ" || x.Місяць == month) &&
                                                                                                 (apartment == "ВСІ" || x.Квартира == apartment))
                                                                                    .Select(x => x)
                                                                                    
                                                            }
                                                            .ToCollection("Показники")
                                                            .SetUIDimension("{'ReadOnly':true,'Style':'HasLabel:false;Hide:Number'}")
                                                            .OpenForm();
                                                        }
                                                    });
                                            }
                                        }));
    }
}
