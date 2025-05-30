using FractalPlatform.Client.UI.DOM;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Common.Clients;
using FractalPlatform.Common.Enums;

namespace FractalPlatform.Cartouche {
    public class CartoucheApplication : DashboardApplication
    {
        public class Comment
        {
            public string Name
            {
                get;
                set;
            }
            public string Text
            {
                get;
                set;
            }
            public DateTime OnDate
            {
                get;
                set;
            }
            public List<string> Likes
            {
                get;
                set;
            }
        }

        public class Post
        {
            public string Name
            {
                get;
                set;
            }
            public string FullName
            {
                get;
                set;
            }
            public string Avatar
            {
                get;
                set;
            }
            public string Text
            {
                get;
                set;
            }
            public string Picture
            {
                get;
                set;
            }
            public DateTime OnDate
            {
                get;
                set;
            }

            public string LikePost
            {
                get;
                set;
            }
            
            public string EditPost
            {
                get;
                set;
            }

            public List<string> Likes
            {
                get;
                set;
            }
            public List<Comment> Comments
            {
                get;
                set;
            }
        }

        private class Bot
        {
            public string Name
            {
                get;
                set;
            }

            public string FullName
            {
                get;
                set;
            }

            public string Avatar
            {
                get;
                set;
            }

            public string Prompt
            {
                get;
                set;
            }
        }

        private void NewPost()
        {
            CreateNewDocFor("NewPost", "Posts")
                        .OpenForm(result =>
                        {
                            if (result.Result)
                            {
                                ProcessBots(result.TargetDocID);
                            }

                            Dashboard();
                        });
        }

        private void Dashboard()
        {
            CloseIfOpenedForm("Dashboard");

            var following = DocsWhere("Users", "{'Name':@UserName}")
                .Values("{'Following':[$]}");

            following.Add(Context.User.Name);

            //var posts = DocsWhere("Posts", "{'Name':Any(@Following)}", following)
            var posts = DocsOf("Posts")
                .Select<Post>()
                .Select(x => new
                {
                    Name = x.Name,
                    FullName = x.FullName,
                    Text = x.Text,
                    Picture = x.Picture,
                    Avatar = x.Avatar,
                    OnDate = x.OnDate,
                    LikePost = x.LikePost,
                    EditPost = x.Name == Context.User.Name ? "Edit Post" : null,
                    Likes = x.Likes.Count,
                    Comments = x.Comments.Count
                })
                .OrderByDescending(x => x.OnDate)
                .Take(50)
                .ToList();

            if (!posts.Any())
            {
                NewPost();

                return;
            }

            var values = DocsWhere("Users", "{'Name':@UserName}")
                            .Values("{'FullName':$,'Avatar':$}");

            FirstDocOf("Dashboard")
                .ToCollection()
                .DeleteByParent("Posts")
                .ExtendDocument(DQL("{'FullName':@FullName,'Avatar':@Avatar}", values[0], values[1]))
                .MergeToArrayPath(posts, "Posts", Constants.FIRST_DOC_ID, true)
                .OpenForm();
        }

        public override void OnStart()
        {
            if (Context.UrlTag == "settings")
            {
                ModifyFirstDocOf("Settings")
                    .OpenForm();
            }
            else
            {
                base.OnStart();
            }
        }

        public override void OnRegister(FormResult result) => TryAutoLogin();

        public override void OnLogin(FormResult result) => Dashboard();

        public override bool OnEventDimension(EventInfo info)
        {

            var action = info.Action;

            switch (action)
            {
                case @"Register":
                    {
                        CreateNewDocFor("NewUser", "Users")
                            .OpenForm(result =>
                            {
                                if (result.Result)
                                {
                                    MessageBox("You are registered !", MessageBoxButtonType.Ok, result => Login());
                                }
                                else
                                {
                                    Login();
                                }
                            });

                        break;
                    }
                case @"EditUser":
                    {
                        DocsWhere("Users", "{'Name':@UserName}")
                            .OpenForm(result =>
                            {
                                if (result.Result)
                                {
                                    Context.User.Avatar = result.FindFirstValue("Avatar");
                                }
                            });

                        break;
                    }
                case @"SignOut":
                    {

                        Logout();

                        break;
                    }
                case @"NewPost":
                    {
                        NewPost();

                        break;
                    }
                case @"EditBots":
                    {
                        ModifyDocsWhere("Users", "{'IsBot':true}")
                            .SetDimension(DimensionType.Filter, "{}")
                            .ExtendUIDimension("{'Settings':{'Visible':true}}")
                            .OpenForm();

                        break;
                    }
                case @"NewBot":
                    {
                        CreateNewDocFor("NewUser", "Users")
                            .ExtendDocument("{'IsBot':true}")
                            .ExtendUIDimension("{'Style':'Save:Create;CollLabel:New bot','Category':{'Visible':true},'Prompt':{'Visible':true},'Settings':{'Visible':true},'Password':{'Visible':false}}")
                            .OpenForm(result =>
                            {
                                if (result.Result)
                                {
                                    MessageBox("You are added new bot !",
                                               MessageBoxButtonType.Ok,
                                               result => Dashboard());
                                }
                                else
                                {
                                    Dashboard();
                                }
                            });

                        break;
                    }
                case @"NewComment":
                    {
                        CreateNewDocForArray("NewComment", "Posts", "{'Comments':[$]}", info.DocID)
                            .OpenForm();

                        break;
                    }
                case @"Follow":
                    {
                        var name = DocsWhere("Posts", info.AttrPath)
                                        .Value("{'Name':$}");

                        DocsWhere("Users", "{'Name':@UserName}")
                            .Update("{'Following':[Add,@Name]}", name);

                        break;
                    }
                case @"FollowUser":
                    {
                        var name = DocsWhere("Users", info.AttrPath)
                                        .Value("{'Name':$}");

                        if (!DocsWhere("Users", "{'Name':@UserName,'Following':[Any,@name]}", name)
                            .Any())
                        {
                            DocsWhere("Users", "{'Name':@UserName}")
                                .Update("{'Following':[Add,@Name]}", name);
                        }

                        break;
                    }
                case @"Unfollow":
                    {
                        var name = DocsWhere("Posts", info.AttrPath)
                                        .Value("{'Name':$}");

                        DocsWhere("Users", "{'Name':@UserName}")
                            .Delete("{'Following':[@Name]}", name);

                        break;
                    }
                case @"LikeComment":
                    {

                        var query = DocsWhere("Posts", info.AttrPath)
                                        .AndWhere("{'Comments':[{'Likes':[Any,@UserName]}]}");

                        if (!query.Any())
                        {
                            DocsWhere("Posts", info.AttrPath)
                                .Update("{'Comments':[{'Likes':[Add,@UserName]}]}");
                        }
                        else
                        {
                            DocsWhere("Posts", info.AttrPath)
                                .Delete("{'Comments':[{'Likes':[@UserName]}]}");
                        }

                        DocsWhere("Posts", info.DocID)
                            .OpenForm(result => Dashboard());

                        break;
                    }
                case @"Reply":
                    {

                        var name = DocsWhere("Posts", info.AttrPath)
                                      .Value("{'Comments':[{'Name':$}]}");

                        CreateNewDocForArray("NewComment", "Posts", "{'Comments':[$]}", info.DocID)
                            .ExtendDocument(DQL("{'Text':@Text}", name))
                            .OpenForm();

                        break;
                    }
                case @"LikePost":
                    {

                        if (info.Collection.Name == "Dashboard")
                        {
                            var nameAndOnDate = info.Collection
                                                    .GetWhere(info.AttrPath)
                                                    .Values("{'Posts':[{'Name':$,'OnDate':$}]}");

                            var docID = DocsWhere("Posts", "{'Name':@Name,'OnDate':@OnDate}", nameAndOnDate[0], nameAndOnDate[1])
                                            .GetFirstID();

                            var query = DocsWhere("Posts", docID)
                                            .AndWhere("{'Likes':[Any,@UserName]}");

                            if (!query.Any())
                            {
                                DocsWhere("Posts", docID)
                                    .Update("{'Likes':[Add,@UserName]}");
                            }
                            else
                            {
                                DocsWhere("Posts", docID)
                                    .Delete("{'Likes':[@UserName]}");
                            }

                            Dashboard();
                        }
                        else
                        {
                            var query = DocsWhere("Posts", info.DocID)
                                        .AndWhere("{'Likes':[Any,@UserName]}");

                            if (!query.Any())
                            {
                                DocsWhere("Posts", info.DocID)
                                    .Update("{'Likes':[Add,@UserName]}");
                            }
                            else
                            {
                                DocsWhere("Posts", info.DocID)
                                    .Delete("{'Likes':[@UserName]}");
                            }

                            DocsWhere("Posts", info.DocID)
                                .OpenForm(result => Dashboard());
                        }

                        break;
                    }
                case @"EditPost":
                    {
                        if (info.Collection.Name == "Dashboard")
                        {
                            var nameAndOnDate = info.Collection
                                                    .GetWhere(info.AttrPath)
                                                    .Values("{'Posts':[{'Name':$,'OnDate':$}]}");

                            var docID = DocsWhere("Posts", "{'Name':@Name,'OnDate':@OnDate}", nameAndOnDate[0], nameAndOnDate[1])
                                    .GetFirstID();
                                    
                            ModifyDocsWhere("Posts", docID)
                                .ExtendUIDimension("{'Style':'Save:Update','IsRawPage':false,'Layout':'','Visible':false,'Text':{'Visible':true},'Picture':{'Visible':true}}")
                                .OpenForm(result => Dashboard());

                        }
                        else
                        {
                            ModifyDocsWhere("Posts", info.DocID)
                                .ExtendUIDimension("{'Style':'Save:Update','IsRawPage':false,'Layout':'','Visible':false,'Text':{'Visible':true},'Picture':{'Visible':true}}")
                                .OpenForm();
                        }

                        break;
                    }
                case @"Comments":
                    {
                        var nameAndOnDate = info.Collection
                                                .GetWhere(info.AttrPath)
                                                .Values("{'Posts':[{'Name':$,'OnDate':$}]}");

                        var docID = DocsWhere("Posts", "{'Name':@Name,'OnDate':@OnDate}", nameAndOnDate[0], nameAndOnDate[1])
                                        .GetFirstID();

                        DocsWhere("Posts", docID)
                            .OpenForm(result => Dashboard());

                        break;
                    }
                default:
                    {
                        return base.OnEventDimension(info);
                    }
            }

            return true;
        }

        public override bool OnSecurityDimension(SecurityInfo info)
        {
            var result = false;

            switch (info.Variable)
            {
                case @"Follow":
                    {
                        var name = DocsWhere("Posts", info.AttrPath)
                                        .Value("{'Name':$}");

                        result = (!DocsWhere("Users", "{'Name':@UserName}")
                                    .AndWhere("{'Following':[Any,@Name]}", name)
                                    .Any()) && Context.User.Name != name;
                        break;
                    }
                case @"Unfollow":
                    {
                        var name = DocsWhere("Posts", info.AttrPath)
                                        .Value("{'Name':$}");

                        result = DocsWhere("Users", "{'Name':@UserName}")
                                    .AndWhere("{'Following':[Any,@Name]}", name)
                                    .Any() && Context.User.Name != name; ;

                        break;
                    }
                default:
                    {
                        return base.OnSecurityDimension(info);
                    }
            }

            return result;
        }

        public void ProcessBots(uint docID)
        {
            return;

            var text = DocsWhere("Posts", docID)
                        .Value("{'Text':$}");

            var random = new Random();
            int count = random.Next(2, 4);

            var bots = DocsWhere("Users", "{'IsBot':true,'IsActive':true}")
                        .Select<Bot>()
                        .OrderBy(x => Guid.NewGuid())
                        .Take(count);

            foreach (var bot in bots)
            {
                var prompt = $"Ты {bot.Prompt}. Сгенерируй короткое сообщение до 400 символов как ответ на это сообщение {text}";

                var aiText = AI.Generate(prompt, AIModel.GPT4oMini).Text;

                Log("Processed bot => " + bot.Name);
                Log("Message => " + aiText);

                DocsWhere("Posts", docID)
                    .Update("{'Comments':[Add,@Comment]}",
                        new
                        {
                            Name = bot.Name,
                            Avatar = bot.Avatar,
                            FullName = bot.FullName,
                            Text = aiText,
                            OnDate = DateTime.Now
                        });
            }
        }

        public override object OnComputedDimension(ComputedInfo info)
        {
            object result = null;

            switch (info.Variable)
            {
                case @"ValidatePostName":
                {
                    result = DocsWhere("Users", "{'Name':@Name}", info.AttrValue)
                                .Any();
            
                    break;
                }
                case @"ValidateCommentName":
                {
                    result = DocsWhere("Users", "{'Name':@Name}", info.AttrValue)
                                .Any();
            
                    break;
                }
                case @"ValidateLikePost":
                {
                    result = DocsWhere("Users", "{'Name':@Name}", info.AttrValue).Any() &&
                            !DocsWhere("Posts", info.AttrPath.DocID)
                                .AndWhere("{'Likes':[Any,@Name]}", info.AttrValue)
                                .Any();
                    
                    break;
                }
                case @"ValidateFollowUser":
                {
                    result = DocsWhere("Users", "{'Name':@Name}", info.AttrValue).Any() &&
                            !DocsWhere("Users", info.AttrPath.DocID)
                            .AndWhere("{'Following':[Any,@Name]}", info.AttrValue)
                            .Any();

                    break;
                }
                case @"Text":
                    {
                        var text = System.Net.WebUtility.HtmlEncode(info.AttrValue.ToString());
                        
                        string picture;
                        
                        if(info.Collection.Name == "Dashboard")
                        {
                            picture = info.Collection
                                          .GetWhere(info.AttrPath)
                                          .Value("{'Posts':[{'Picture':$}]}");
                        }
                        else if(info.AttrPath.FirstPath != "Comments")
                        {
                            picture = info.Collection
                                          .GetWhere(info.AttrPath)
                                          .Value("{'Picture':$}");
                        }
                        else
                        {
                            picture = info.Collection
                                          .GetWhere(info.AttrPath)
                                          .Value("{'Comments':[{'Picture':$}]}");
                        }
                        
                        if (!string.IsNullOrEmpty(picture) && picture != "null")
                        {
                            text += $"<br><br><img style='max-width:400px; max-height:300px' src='{GetFileUrl(picture)}'/>";
                        }

                        result = text;
                        
                        break;
                    }
                case @"OnDate":
                    {
                        var dt = info.AttrValue.ToString();

                        var culture = CultureInfo.InvariantCulture;
                        var ts = DateTime.Now.Subtract(DateTime.Parse(dt, culture));

                        if ((int)ts.TotalDays / 365 > 0)
                        {
                            result = "год назад";
                        }
                        else if ((int)ts.TotalDays / 30 > 0)
                        {
                            result = $"{(int)ts.TotalDays / 30} месяцев назад";
                        }
                        else if ((int)ts.TotalDays > 0)
                        {
                            result = $"{(int)ts.TotalDays} дней назад";
                        }
                        else if ((int)ts.TotalHours > 0)
                        {
                            result = $"{(int)ts.TotalHours} часов назад";
                        }
                        else if ((int)ts.TotalMinutes > 0)
                        {
                            result = $"{(int)ts.TotalMinutes} минут назад";
                        }
                        else
                        {
                            result = $"{(int)ts.TotalSeconds} секунд назад";
                        }
                        break;
                    }
                case @"Avatar":
                    {
                        string name;

                        if (info.AttrPath.Count == 1)
                        {
                            name = DocsWhere("Posts", info.AttrPath)
                                       .Value("{'Name':$}");
                        }
                        else
                        {
                            name = DocsWhere("Posts", info.AttrPath)
                                       .Value("{'Comments':[{'Name':$}]}");
                        }

                        result = DocsWhere("Users", "{'Name':@Name}", name)
                                    .Value("{'Avatar':$}");

                        break;
                    }
                case @"FullName":
                    {
                        string name;

                        if (info.AttrPath.Count == 1)
                        {
                            name = DocsWhere("Posts", info.AttrPath)
                                       .Value("{'Name':$}");
                        }
                        else
                        {
                            name = DocsWhere("Posts", info.AttrPath)
                                       .Value("{'Comments':[{'Name':$}]}");
                        }

                        result = DocsWhere("Users", "{'Name':@Name}", name)
                                    .Value("{'FullName':$}");

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