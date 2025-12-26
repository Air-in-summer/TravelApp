using TravelApplication.Pages;
using TravelApplication.Pages.ManagePage;

namespace TravelApplication
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;
            // Kiểm tra đăng nhập
            MainPage = new AppShell();
            var token = SecureStorage.GetAsync("token").Result;
            if (string.IsNullOrEmpty(token))
            {
                // Chưa đăng nhập → chuyển đến LoginPage
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                // Đã đăng nhập → chuyển đến MainPage
                MainPage = new AppShell();

            }
        }
    }

    public static class ServiceHelper
    {
        public static T GetService<T>() where T : class
            => App.Services.GetService<T>();       
    }
}
