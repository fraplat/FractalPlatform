using FractalPlatform.Client.App;
using FractalPlatform.Client.UI;

namespace FractalPlatform.ToDoWithCategories
{
    public class ToDoWithCategoriesApplication : BaseApplication
    {
        public override void OnStart() => ModifyFirstDocOf("ToDoList").OpenForm();
    }
}
