using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.fraplat
{
    public class fraplatApplication : BaseApplication
    {
        public override void OnStart()
        {
            if(!Context.HasUrlTag || Context.UrlTag == "home")
                    FirstDocOf("Home").OpenForm();
                else if(Context.UrlTag == "entry") 
                    FirstDocOf("Entry").OpenForm();
                else if(Context.UrlTag == "basic") 
                    FirstDocOf("Basic").OpenForm();
                else if(Context.UrlTag == "advanced") 
                    FirstDocOf("Advanced").OpenForm();
                else if(Context.UrlTag == "enterprise") 
                    FirstDocOf("Enterprise").OpenForm();
                else if(Context.UrlTag == "pricing") 
                    FirstDocOf("Pricing").OpenForm();
                else if(Context.UrlTag == "about") 
                    FirstDocOf("About").OpenForm();
                else NotImplementedMessageBox(); 
        }
    }
}