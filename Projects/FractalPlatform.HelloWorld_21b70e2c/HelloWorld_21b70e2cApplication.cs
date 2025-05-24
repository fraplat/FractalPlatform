using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.HelloWorld_21b70e2c
{
    public class HelloWorld_21b70e2cApplication : BaseApplication
    {
        public override void OnStart() => ModifyFirstDocOf("ToDoList").OpenForm();
    }
}
