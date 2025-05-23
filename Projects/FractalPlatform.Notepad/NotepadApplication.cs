using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.Notepad
{
    public class NotepadApplication : BaseApplication
    {
        public override void OnStart() =>
            UsePassword("mypass", () => ModifyFirstDocOf("Notes").OpenForm(result => SaveForm()));
    }
}
