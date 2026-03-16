using Microsoft.UI.Xaml;

namespace FoodTourApp.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
            // Bạn có thể xóa dòng RegisterServiceKey cũ đi vì chúng ta dùng WebView rồi
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}