using FractalPlatform.Client.UI.DOM;
using System;
using System.Linq;
using System.Collections.Generic;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Common.Clients;

namespace FractalPlatform.Cartouche {
    public class CartoucheApplication: BaseApplication {
        
        public class Comment {
            public string Name {
                get;
                set;
            }
            public string Text {
                get;
                set;
            }
            public DateTime OnDate {
                get;
                set;
            }
            public int Likes {
                get;
                set;
            }
        }

        public class Post {
            public string Name {
                get;
                set;
            }
            public string Text {
                get;
                set;
            }
            public DateTime OnDate {
                get;
                set;
            }
            public int Likes {
                get;
                set;
            }
            public List<Comment> Comments {
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
            
            public string Prompt
            {
                get;
                set;
            }
        }

        private void Dashboard() {
            CloseIfOpenedForm("Dashboard");
            
            var following = DocsWhere("Users", "{'Name':@UserName}")
                .Values("{'Following':[$]}");

            following.Add(Context.User.Name);

            var posts = DocsWhere("Posts", "{'Name':Any(@Following)}", following)
                .Select<Post>();

            FirstDocOf("Dashboard")
                .ToCollection()
                .DeleteByParent("Posts")
                .MergeToArrayPath(posts, "Posts", Constants.FIRST_DOC_ID, true)
                .OpenForm();
        }
        
        private void Login() {
            FirstDocOf("Login")
                .OpenForm(result => {
                    if (result.Result) {
                        var name = result.FindFirstValue("Login");
                        var password = result.FindFirstValue("Password");

                        if (DocsOf("Users")
                            .GetWhere("{'Name':@Name,'Password':@Password}", name, password)
                            .Any()) {
                            Context.User.Name = name;
                            Dashboard();
                        } else {
                            MessageBox("Wrong credentials.");
                        }
                    }
                });
        }

        public override void OnStart() {
            Login();
        }

        public override bool OnEventDimension(EventInfo info) {

            var path = info.AttrPath.ToString();

            switch (path) {
                case @"Register": {
                    CreateNewDocFor("NewUser", "Users")
                        .OpenForm(result => {
                            if (result.Result) {
                                MessageBox("You are registered !", MessageBoxButtonType.Ok, result => Login());
                            } else {
                                Login();
                            }
                        });

                    break;
                }
                case @"EditUser": {
                    DocsWhere("Users", "{'Name':@UserName}")
                        .OpenForm();

                    break;
                }
                case @"NewPost": {
                    CreateNewDocFor("NewPost", "Posts")
                        .OpenForm(result => 
                        {
                            ProcessBots(result.TargetDocID);
                            
                            Dashboard();
                        });

                    break;
                }
                case @"EditBots": {
                    ModifyDocsWhere("Users", "{'IsBot':true}")
                        .OpenForm();

                    break;
                }
                case @"NewComment": {
                    CreateNewDocForArray("NewComment", "Posts", "{'Comments':[$]}", info.DocID)
                        .OpenForm();

                    break;
                }
                case @"Follow": {
                    var name = DocsWhere("Posts",info.AttrPath)
                                    .Value("{'Name':$}");
                        
                    DocsWhere("Users", "{'Name':@UserName}")        
                        .Update("{'Following':[Add,@Name]}", name);

                    break;
                }
                case @"Unfollow": {
                    var name = DocsWhere("Posts",info.AttrPath)
                                    .Value("{'Name':$}");
                        
                    DocsWhere("Users", "{'Name':@UserName}")        
                        .Delete("{'Following':[Add,@Name]}", name);

                    break;
                }
                default: {
                    return base.OnEventDimension(info);
                }
            }

            return true;
        }
    
        public override bool OnOpenForm(FormInfo info)
        {
            if (info.Collection.Name == "Dashboard" &&
                info.DocID != Constants.ANY_DOC_ID &&
                info.AttrPath.FirstPath == "Posts")
            {
                var docID = info.Collection
                                .GetWhere(info.AttrPath)
                                .UIntValue("{'Posts':[{'DocID':$}]}");
                                
                DocsWhere("Posts", docID).OpenForm();
                
                return false;
            }

            return true;
        }
    
        public override bool OnSecurityDimension(SecurityInfo info)
        {
            var result = false;

            switch(info.Variable)
            {
                case @"Follow":
                {
                    var name = DocsWhere("Posts",info.AttrPath)
                                    .Value("{'Name':$}");
                        
                    result = DocsWhere("Users", "{'Name':@UserName}")        
                                .AndWhere("{'Following':[Any,@Name]}", name)
                                .Any();
                    break;
                }
                case @"Unfollow":
                {
                    var name = DocsWhere("Posts",info.AttrPath)
                                    .Value("{'Name':$}");
                        
                    result = !DocsWhere("Users", "{'Name':@UserName}")        
                                .AndWhere("{'Following':[Any,@Name]}", name)
                                .Any();

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
            var text = DocsWhere("Posts", docID)
                        .Value("{'Text':$}");
            
            var bots = DocsWhere("Users","{'IsBot':true}")
                        .Select<Bot>();
            
            foreach(var bot in bots)
            {
                var prompt = $"Ты {bot.Prompt}. Сгенерируй короткое сообщение до 100 символов как ответ на это сообщение {text}";
                
                var aiText = AI.Generate(prompt, AIModel.GPT4oMini).Text;
                
                Log("Processed bot => " + bot.Name);
                Log("Message => " + aiText);
                
                DocsWhere("Posts", docID)
                    .Update("{'Comments':[Add,@Comment]}",
                        new {Name = bot.Name,
                             Text = aiText,
                             OnDate = DateTime.Now,
                             Likes = 0});
            }
        }
    }
}