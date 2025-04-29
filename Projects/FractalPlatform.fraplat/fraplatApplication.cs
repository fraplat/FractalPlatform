using FractalPlatform.Database.Engine.Info;
using FractalPlatform.Database.Engine;
using FractalPlatform.Client.UI.DOM;
using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.fraplat
{
    public class fraplatApplication : BaseApplication
    {
        public override void OnStart() => ModifyFirstDocOf("Home").OpenForm();
    }
}
