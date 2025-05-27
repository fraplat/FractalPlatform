using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;
using FractalPlatform.Database.Engine;
using FractalPlatform.Database.Engine.Info;

namespace FractalPlatform.Notepad
{
    public class NotepadApplication : BaseApplication
    {
        private AttrPath _attrPath;
        
        public override void OnStart() =>
            UsePassword("mypass", () => 
            {
                ModifyFirstDocOf("Notes")
                    .OpenForm(result => 
                    {
                        SaveForm();
                        
                        if(result.Result)
                        {
                            ModifyDocsWhere("Notes", _attrPath)
                                .OpenForm(result => SaveForm());
                        }
                    });
            });

        public override bool OnCloseForm(FormInfo info)
        {
            _attrPath = info.AttrPath;

            return true;
        }
    }
}