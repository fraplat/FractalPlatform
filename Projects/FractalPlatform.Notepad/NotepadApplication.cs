using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.Notepad
{
    public class NotepadApplication : BaseApplication
    {
        private void Dashboard() => ModifyFirstDocOf("Notes").OpenForm(result => SaveForm());
        
        public override void OnStart()
        {
            var password = "123";
            
            if(Context.UrlTag == password)
            {
                Dashboard();
            }
            else
            {
                InputBox("Password", "Enter password", result =>
                {
                    if (result.Result)
                    {
                        var currPassword = result.FindFirstValue("Password");
                        
                        if (currPassword == password)
                        {
                            Context.UrlTag = currPassword;
                            
                            Dashboard();
                        }
                        else
                        {
                            MessageBox("Wrong credentials.");
                        }
                    }
                });
            }
        }
    }
}
