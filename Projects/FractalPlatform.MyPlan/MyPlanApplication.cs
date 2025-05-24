using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using FractalPlatform.Database.Engine;
using FractalPlatform.Common.Enums;
using FractalPlatform.Client.UI;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Client.App;

namespace FractalPlatform.MyPlan 
{
    public class TaskInfo
    {
        public int Completed {get; set;}
        public bool IsCompleted {get; set;}
        public int Points {get; set;}
        public string RepeatEvery {get; set;}
        public string Task {get; set;}
        public string Time {get; set;}
        public string Description {get; set;}
        public DateTime DueDate {get; set;}
        public string Remaining {get; set;}
        public DateTime LastDone {get; set;}
        public string Status {get; set;}
        public DateTime StartFrom {get; set;}
    }
    
    public class CategoryInfo
    {
        public string Name {get; set;}
        public int Priority {get; set;}
        
        public List<TaskInfo> Tasks {get; set;}
    }
    
    public class MyPlanApplication: BaseApplication 
    {
        private CategoryInfo[] GetCategories()
        {
            return FirstDocOf("Categories")
                    .Select<CategoryInfo>("{'Categories':[!$]}");
        }
        
        private void Dashboard()
        {
            FirstDocOf("Dashboard")
                .OpenForm();
        }
        
        private void AddReport(TaskInfo[] repeat,
                               TaskInfo[] never,
                               TaskInfo[] once,
                               TaskInfo[] week,
                               TaskInfo[] month,
                               TaskInfo[] year,
                               bool isYesterday)
        {
            var tasks = repeat.Union(never)
                              .Union(once)
                              .Union(week)
                              .Union(month)
                              .Union(year);
            
            var points = 0;
            
            var time = new TimeSpan();
            
            DateTime now;
            
            if (!isYesterday)
            {
                now = DateTime.Now;
            }
            else
            {
                now = DateTime.Now.AddDays(-1);
            }
            
            var onDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            foreach(var task in tasks)
            {
                if(task.RepeatEvery == "Once")
                {
                    task.Status = "Closed";
                }
                
                ModifyDocsWhere("Categories", "{'Categories':[{'Tasks':[{'Task':@Task}]}]}", task.Task)
                    .Update("{'Categories':[{'Tasks':[{'Completed':Add(1),'LastDone':@NowDate,'Status':@Status}]}]}", now, task.Status);
                
                task.LastDone = onDate;
                
                points += task.Points;
                
                if(task.Time.EndsWith("m"))
                {
                    time = time.Add(TimeSpan.FromMinutes(double.Parse(task.Time.Replace("m",""))));
                }
                else if(task.Time.EndsWith("h"))
                {
                    time = time.Add(TimeSpan.FromHours(double.Parse(task.Time.Replace("h",""))));
                }
                else if(task.Time.EndsWith("d"))
                {
                    time = time.Add(TimeSpan.FromDays(double.Parse(task.Time.Replace("d",""))));
                }
            }
            
            //add report
            var report = new 
            {
                OnDate = onDate,
                Day = onDate.DayOfWeek.ToString(),
                TotalTime = time.ToString(),
                TotalPoints = points,
                Repeat = repeat,
                Never = never,
                Once = once,
                Week = week,
                Month = month,
                Year = year
            };
            
            if(!DocsWhere("Report", "{'OnDate':@OnDate}", onDate).Any())
            {
                AddDoc("Report", report);    
                
                MessageBox($"Today you have {points} points. You completed {time} time of work",
                            "Your score",
                            MessageBoxButtonType.Ok,
                            result => Dashboard());
            }
            else
            {
                DocsWhere("Report", "{'OnDate':@OnDate}", onDate)
                    .Merge(report.ToJson());
                
                MessageBox($"You added {points} points. You completed {time} time of work",
                            "Your score",
                            MessageBoxButtonType.Ok,
                            result => Dashboard());
            }
        }
        
        public override void OnStart() => UsePassword("77", () => Today());
        
        public override object OnComputedDimension(ComputedInfo info)
        {
            var result = 0;
            
            switch(info.Variable)
            {
                case @"TotalSumVariable":
                {
                    result = info.Collection
                                 .ToAttrList()
                                 .Where(x => x.Key.LastPath == "Sum")
                                 .Sum(x => x.Value.GetIntValue());

                    break;
                }
                case @"TotalCacheVariable":
                {
                    result = info.Collection
                                 .ToAttrList()
                                 .Where(x => x.Key.HasPath("Cache") && x.Key.LastPath == "Sum")
                                 .Sum(x => x.Value.GetIntValue());

                    break;
                }
                case @"TotalBankVariable":
                {
                    result = info.Collection
                                 .ToAttrList()
                                 .Where(x => x.Key.HasPath("Bank") && x.Key.LastPath == "Sum")
                                 .Sum(x => x.Value.GetIntValue());

                    break;
                }
                case @"TotalCryptoVariable":
                {
                    result = info.Collection
                                 .ToAttrList()
                                 .Where(x => x.Key.HasPath("Crypto") && x.Key.LastPath == "Sum")
                                 .Sum(x => x.Value.GetIntValue());

                    break;
                }
                case @"TotalRealEstateVariable":
                {
                    result = info.Collection
                                 .ToAttrList()
                                 .Where(x => x.Key.HasPath("RealEstate") && x.Key.LastPath == "Sum")
                                 .Sum(x => x.Value.GetIntValue());

                    break;
                }
                case "TotalPointsVariable":
                {
                    return  DocsWhere("Report", info.AttrPath).Sum("{'Repeat':[{'Points':$}]}") +
                            DocsWhere("Report", info.AttrPath).Sum("{'Never':[{'Points':$}]}") +
                            DocsWhere("Report", info.AttrPath).Sum("{'Once':[{'Points':$}]}") +
                            DocsWhere("Report", info.AttrPath).Sum("{'Week':[{'Points':$}]}") +
                            DocsWhere("Report", info.AttrPath).Sum("{'Month':[{'Points':$}]}") +
                            DocsWhere("Report", info.AttrPath).Sum("{'Year':[{'Points':$}]}");
                }
                case "TotalTimeVariable":
                {
                    var currTimes = DocsWhere("Report", info.AttrPath).Values("{'Repeat':[{'Time':$}]}")
                                    .Union(DocsWhere("Report", info.AttrPath).Values("{'Never':[{'Time':$}]}"))
                                    .Union(DocsWhere("Report", info.AttrPath).Values("{'Once':[{'Time':$}]}"))
                                    .Union(DocsWhere("Report", info.AttrPath).Values("{'Week':[{'Time':$}]}"))
                                    .Union(DocsWhere("Report", info.AttrPath).Values("{'Month':[{'Time':$}]}"))
                                    .Union(DocsWhere("Report", info.AttrPath).Values("{'Year':[{'Time':$}]}"));
                    
                    var totalTime = new TimeSpan();
                    
                    foreach(var time in currTimes)
                    {
                        if(time.EndsWith("m"))
                        {
                            totalTime = totalTime.Add(TimeSpan.FromMinutes(double.Parse(time.Replace("m",""))));
                        }
                        else if(time.EndsWith("h"))
                        {
                            totalTime = totalTime.Add(TimeSpan.FromHours(double.Parse(time.Replace("h",""))));
                        }
                        else if(time.EndsWith("d"))
                        {
                            totalTime = totalTime.Add(TimeSpan.FromDays(double.Parse(time.Replace("d",""))));
                        }
                    }
                    
                    return $"[{totalTime}]";
                }
                default:
                {
                    return base.OnComputedDimension(info);
                }
            }

             return result;
        }
        
        private int GetDayOfWeekInt(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek != DayOfWeek.Sunday)
            {
                return (int)dayOfWeek - 1;
            }
            else
            {
                return 6;    
            }
        }
        
        private void Today(bool isYesterday = false)
        {
            DateTime now;
            
            if (!isYesterday)
            {
                now = DateTime.Now;
            }
            else
            {
                now = DateTime.Now.AddDays(-1);
            }
            
            var startDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            var startWeek = startDay.AddDays(-GetDayOfWeekInt(startDay.DayOfWeek));
            var startMonth = new DateTime(startDay.Year, startDay.Month, 1);
            var startYear = new DateTime(startDay.Year, 1, 1);
            var isWorkingDay = GetDayOfWeekInt(startDay.DayOfWeek) >= GetDayOfWeekInt(DayOfWeek.Monday) &&
                               GetDayOfWeekInt(startDay.DayOfWeek) <= GetDayOfWeekInt(DayOfWeek.Friday);

            var cats = GetCategories();
            var tasks = cats.SelectMany(c => c.Tasks
                                              .Where(t => startDay > t.StartFrom && (t.Status == "Active" || t.Status == "InProgress"))
                                              .Select(t => new
                                              {
                                                   Category = c.Name,
                                                   Priority = c.Priority,
                                                   Task = t.Task,
                                                   RepeatEvery = t.RepeatEvery,
                                                   Completed = t.Completed,
                                                   Points = t.Points,
                                                   Time = t.Time,
                                                   LastDone = t.LastDone,
                                                   DueDate = t.DueDate,
                                                   Remaining = t.Remaining,
                                                   Status = t.Status,
                                                   StartFrom = t.StartFrom
                                              }));
                                              
            var lostedWeekTasks = tasks.Where(t => 
            {
                DayOfWeek dayOfWeek;
                
                return Enum.TryParse(t.RepeatEvery, out dayOfWeek) &&
                       GetDayOfWeekInt(dayOfWeek) < GetDayOfWeekInt(now.DayOfWeek) &&
                       t.LastDone < startWeek;
            });
            
            var repeat = tasks.Where(t => (t.RepeatEvery == "Day" ||
                                           t.RepeatEvery == now.DayOfWeek.ToString() ||
                                           (t.RepeatEvery == "WorkingDay" && isWorkingDay)) &&
                                           t.LastDone < startDay)
                              .OrderBy(t => t.Priority)
                              .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        Status = t.Status
                                    });
                                    
            var never = tasks.Where(t => t.RepeatEvery == "Never" && t.LastDone < startDay)
                             .OrderBy(t => t.Priority)
                             .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        Status = t.Status
                                    });
                                    
            var once = tasks.Where(t => t.RepeatEvery == "Once" && t.Completed == 0)
                            .OrderBy(t => t.Priority)
                            .ThenBy(t => t.DueDate)
                            .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        DueDate = t.DueDate,
                                        Remaining = t.Remaining,
                                        Status = t.Status
                                    });
            
            var week = tasks.Where(t => t.RepeatEvery == "Week" && t.LastDone < startWeek)
                            .Union(lostedWeekTasks)
                            .OrderBy(t => t.Priority)
                            .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        Status = t.Status
                                    });
            
            var month = tasks.Where(t => (t.RepeatEvery == "Month" &&
                                          t.LastDone < startMonth) ||
                                         (t.RepeatEvery == "DayOfMonth" &&
                                          t.LastDone < startMonth &&
                                          now.Day >= t.StartFrom.Day))
                            .OrderBy(t => t.Priority)
                            .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        Status = t.Status
                                    });
                                    
            var year = tasks.Where(t => (t.RepeatEvery == "Year" &&
                                          t.LastDone < startYear &&
                                          now > startYear) ||
                                         (t.RepeatEvery == "DayOfYear" &&
                                          t.LastDone < startYear &&
                                          now.Day >= t.StartFrom.Day &&
                                          now.Month >= t.StartFrom.Month))
                            .OrderBy(t => t.Priority)
                            .Select(t => new 
                                    {
                                        Category = t.Category,
                                        Task = t.Task,
                                        Points = t.Points,
                                        IsCompleted = false,
                                        Time = t.Time,
                                        Status = t.Status
                                    });
            
            var obj = new 
            {
                RepeatLabel = repeat.Any() ? "Repeat" : null,
                Repeat = repeat,
                NeverLabel = never.Any() ? "Never" : null,
                Never = never,
                OnceLabel = once.Any() ? "Once" : null,
                Once = once,
                WeekLabel = week.Any() ? "Week" : null,
                Week = week,
                MonthLabel = month.Any() ? "Month" : null,
                Month = month,
                YearLabel = year.Any() ? "Year" : null,
                Year = year
            };
            
            var doc = FirstDocOf("Today")
                        .ExtendDocument(obj.ToJson());
            
            if(now.Hour < 21 &&
               !isYesterday)
            {
                doc.ExtendUIDimension("{'ReadOnly':true}");
            }
                        
            doc.OpenForm(result => 
            {
                if(result.Result)
                {
                    var repeat = result.Collection.GetWhere("{'Repeat':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Repeat':[!$]}");
                    var never = result.Collection.GetWhere("{'Never':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Never':[!$]}");
                    var once = result.Collection.GetWhere("{'Once':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Once':[!$]}");
                    var week = result.Collection.GetWhere("{'Week':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Week':[!$]}");
                    var month = result.Collection.GetWhere("{'Month':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Month':[!$]}");
                    var year = result.Collection.GetWhere("{'Year':[{'IsCompleted':true}]}").Select<TaskInfo>("{'Year':[!$]}");
                    
                    AddReport(repeat, never, once, week, month, year, isYesterday);
                }
                else
                {
                    Dashboard();
                }
            });
        }
        
        private void Report()
        {
            var number = 0;
            var columns = DocsOf("Report")
            			.Values("{'TotalPoints':$}")
            			.Select(val => new { Name = ++number,
            								  Value = double.Parse(val) })
            			.GroupBy(x => x.Name / 14)
            			.Select(g => new {
                            Name = g.Key,
                            Value = g.Average(x => x.Value)
                        })
                        .ToList();
            new 
            {
            	Control = new 
            	{
            		Title = new 
            		{
            			Name = "MyChart",
            			X = "Two weeks",
            			Y = "Points"
            		},
            		Columns = columns
            	}
            }
            .ToCollection("Report")
            .SetUIDimension("{'ReadOnly':true,'Control':{'ControlType':'Chart','Style':'Type:Bar'}}")
            .OpenForm();
            
        }
        
        private void Categories()
        {
            ModifyFirstDocOf("Categories")
               .OpenForm();
        }
        
        private void History()
        {
            DocsOf("Report")
                .OpenForm();
        }
        
        private void Think()
        {
            DocsOf("Think")
                .OpenForm();
        }
        
        public override bool OnEventDimension(EventInfo info)
        {

            var path = info.AttrPath.ToString();

            switch(path)
            {
                case @"Categories":
                {
                    Categories();

                    break;
                }
                case @"Report":
                {
                    Report();

                    break;
                }
                case @"Today":
                {
                    Today();

                    break;
                }
                case @"Yesterday":
                {
                    Today(true);

                    break;
                }
                case @"History":
                {
                    History();

                    break;
                }
                case @"Think":
                {
                    Think();
                    
                    break;
                }
                default:
                {
                    return base.OnEventDimension(info);
                }
            }

             return true;
        }
        
        public override BaseRenderForm CreateRenderForm(DOMForm form) => 
                new ExtendedRenderForm(this, form);
    }
}