using System;
using System.Linq;
using System.Collections.Generic;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

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

        private void Dashboard(string name) {
            var following = DocsWhere("Users", "{'Name':@UserName}")
                .Values("{'Following:[$]'}");

            var posts = DocsWhere("Posts", "{'Name':[Any,@Following]}", following)
                .Select<Post>();

            FirstDocOf("Dashboard")
                .ToCollection()
                .DeleteByParent("Posts")
                .MergeToArrayPath(posts, "Posts")
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
                            Dashboard(name);
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
                    CreateNewDocFor("NewUser", "Users")
                        .OpenForm();

                    break;
                }
                case @"EditBots": {
                    ModifyDocsWhere("Bots", "{'IsBot':true}")
                        .OpenForm();

                    break;
                }
                case @"NewComment": {
                    CreateNewDocForArray("NewComment", "Posts", "{'Comments':[$]}", info.DocID)
                        .OpenForm();

                    break;
                }
                case @"Follow": {
                    ModifyDocsOf("Users")
                        .Update("{'Following':[Add,@UserName]}");

                    break;
                }
                case @"Unfollow": {
                    ModifyDocsOf("Users")
                        .Delete("{'Following':[@UserName]}");

                    break;
                }
                default: {
                    return base.OnEventDimension(info);
                }
            }

            return true;
        }
    }
}