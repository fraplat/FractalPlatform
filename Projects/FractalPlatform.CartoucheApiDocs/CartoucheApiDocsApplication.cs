using System.Linq;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;

namespace FractalPlatform.CartoucheApiDocs
{
    public class CartoucheApiDocsApplication : BaseApplication
    {
        public override void OnStart()
        {
            FirstDocOf("ApiDoc")
                .OpenForm();    
        }
    }
}
