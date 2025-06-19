using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Database.Engine;
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
                FirstDocOf("Home")
                    .OpenForm();
            }
            else if(Context.UrlTag == "audit")
            {
                CreateNewDocFor("NewAudit","Audits")
                    .OpenForm(result => FirstDocOf("AuditRequested")
                                            .OpenForm());
            }
            else if(Context.UrlTag == "audits")
            {
                DocsOf("Audits")
                    .OpenForm();
            }
        }
    }
}