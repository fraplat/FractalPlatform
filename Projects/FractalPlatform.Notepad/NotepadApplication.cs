using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.Notepad
{
    public class NotepadApplication : BaseApplication
    {
        public override void OnStart() =>
            UsePasword("mypass", _ => ModifyFirstDocOf("Notes").OpenForm(result => SaveForm()));
    }
}
