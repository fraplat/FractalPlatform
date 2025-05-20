using System.Linq;
using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.agency
{
    public class agencyApplication : BaseApplication
    {
        public override void OnStart()
        {
            if(!Context.HasUrlTag ||
                Context.UrlTag == "home")
            {
                FirstDocOf("Home").OpenForm();
            }
            else if(Context.UrlTag == "audit")
            {
                CreateNewDocFor("NewAudit","Audits").OpenForm();
            }
        }
    }
}