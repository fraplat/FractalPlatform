using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.PresentationGF
{
    public class PresentationGFApplication : BaseApplication
    {
        public override void OnStart()
        {
            ModifyFirstDocOf("Presentation").OpenForm();
        }
    }
}
