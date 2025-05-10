using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.agency
{
    public class agencyApplication : BaseApplication
    {
        public override void OnStart() => FirstDocOf("Home").OpenForm();
    }
}
