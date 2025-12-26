using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine.Info;

namespace FractalPlatform.Notepad
{
    public class NotepadApplication : BaseApplication
    {
        public override void OnStart() => ModifyFirstDocOf("Notes").OpenForm();
    
        public override bool OnEventDimension(EventInfo info)
        {
            var path = info.Action;

            switch(path)
            {
                case @"Cancel":
                {
                    SaveForm();
                    
                    CloseForm();

                    break;
                }
                case @"Save":
                {
                    SaveForm();

                    break;
                }
                case @"Refresh":
                {
                    RefreshForm();

                    break;
                }
                default:
                {
                    return base.OnEventDimension(info);
                }
            }

            return true;
        }
    }
}