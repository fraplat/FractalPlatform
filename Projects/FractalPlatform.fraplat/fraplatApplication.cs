using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.fraplat {
    public class fraplatApplication: BaseApplication {
        public override void OnStart() {
            if (!Context.HasUrlTag || Context.UrlTag == "home") {
                FirstDocOf("Home").OpenForm();
                return;
            }

            switch (Context.UrlTag) {
                case "entry":
                    FirstDocOf("Entry").OpenForm();
                    break;
                case "basic":
                    FirstDocOf("Basic").OpenForm();
                    break;
                case "advanced":
                    FirstDocOf("Advanced").OpenForm();
                    break;
                case "enterprise":
                    FirstDocOf("Enterprise").OpenForm();
                    break;
                case "pricing":
                    FirstDocOf("Pricing").OpenForm();
                    break;
                case "about":
                    FirstDocOf("About").OpenForm();
                    break;
                case "realworld":
                    FirstDocOf("RealWorld").OpenForm();
                    break;
                case "performance":
                    FirstDocOf("Performance").OpenForm();
                    break;
                case "hosting":
                    FirstDocOf("Hosting").OpenForm();
                    break;
                case "storage":
                    FirstDocOf("Storage").OpenForm();
                    break;
                case "autotesting":
                    FirstDocOf("AutoTesting").OpenForm();
                    break;
                default:
                    NotImplementedMessageBox();
                    break;
            }
        }
    }
}