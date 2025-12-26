using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.ListOfProjects
{
    public class ListOfProjectsApplication : BaseApplication
    {
        public override void OnStart() => ModifyFirstDocOf("Projects").OpenForm();
    }
}