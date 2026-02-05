using FractalPlatform.Database.Storages.Internal;

namespace FractalPlatform.MAUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            var storage = new BUFStorage();
            storage.Init();
            storage.FromJson("{ \"Name\": \"John Doe\", \"Age\": 30 }");

            var name = storage.FindFirstAttrValue("Name");

			if (count == 1)
                CounterBtn.Text = $"Clicked {count} time {name}";
            else
                CounterBtn.Text = $"Clicked {count} times {name}";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
