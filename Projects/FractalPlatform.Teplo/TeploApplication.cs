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
                                        .OpenForm(result =>
                                        {
                                            var year = DateTime.Now.Year;
                                            var month = DateTime.Now.ToString("MMMM", new CultureInfo("uk-UA"));
                                            var apartment = result.Collection.FindFirstValue("Квартира");
                                            var value = result.Collection.FindFirstValue("Показник");
                                            
                                            Log(apartment);
                                            
                                            if(result.Result)
                                            {
                                                var query = ModifyDocsWhere("Accounts",
                                                                     "{'Показники':[{'Рік':@Year,'Місяць':@Month,'Квартира':@Apartment}]}",
                                                                      year, month, apartment);
                                                             
                                                if(query.Any())
                                                {
                                                    query.Update("{'Показники':[{'Показник':@Value}]}", value);
                                                    
                                                    MessageBox("Дякуємо, Ваші показники оновлено !", "Внесення показників", MessageBoxButtonType.Ok);
                                                }
                                                else
                                                {
                                                    ModifyFirstDocOf("Accounts")
                                                        .Update("{'Показники':[Add,{'Рік':@Year,'Місяць':@Month,'Квартира':@Apartment,'Показник':@Value}]}",
                                                                 year, month, apartment, value);
                                                    
                                                    MessageBox("Дякуємо, Ваші показники внесено !", "Внесення показників", MessageBoxButtonType.Ok);
                                                }
                                            }
                                        }));
    
        private void History()
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
                                                
                        if(accounts.Показники.Any())
                        {
                            new 
                            {
                                Показники = accounts.Показники
                                                .Where(x => (year == "ВСІ" || x.Рік == year) &&
                                                 (month == "ВСІ" || x.Місяць == month) &&
                                                 (apartment == "ВСІ" || x.Квартира == apartment))
                                                .Select(x => x)
                            }
                            .ToCollection("Показники")
                            .SetUIDimension("{'ReadOnly':true,'Style':'Cancel:Закрити;HasLabel:false;Hide:Number'}")
                            .OpenForm();
                        }
                        else
                        {
                            MessageBox("Немає данних для відображення.",
                                       "Отримання данних",
                                       MessageBoxButtonType.Ok,
                                       result => History());
                        }
                    }
                });
        }
        
        private void NoApartmentsInMonth()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.ToString("MMMM", new CultureInfo("uk-UA"));

            var apartments = DocsWhere("Accounts", "{'Year':@Year,'Month':@Month}", year, month)
                                .IntValues("{'Показники':[{'Квартира':$}]}");

            var noApartments = Enumerable.Range(1, 149)
                                         .Except(apartments)
                                         .ToList();

            MessageBox(
                string.Join(", ", noApartments),
                "Список квартир що не подали показники",
                MessageBoxButtonType.Ok
            );
        }
        
        public override bool OnEventDimension(EventInfo info)
        {
            var path = info.AttrPath.ToString();

            switch(path)
            {
                case @"History":
                {
                    History();

                    break;
                }
                case @"NoApartmentsInMonth":
                {
                    NoApartmentsInMonth();

                    break;
                }
                default:
                {
                    return base.OnEventDimension(info);
                }
            }

             return true;
        }
    }
}