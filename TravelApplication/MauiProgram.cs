using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using System.IO;
using System.Reflection;
using TravelApplication.Pages.MajorPage;
using TravelApplication.Pages.ManagePage;
using TravelApplication.Services;
using TravelApplication.ViewModels;
using Mapsui.UI.Maui;
namespace TravelApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            var getAssembly = Assembly.GetExecutingAssembly();
            using var stream = getAssembly.GetManifestResourceStream("TravelApplication.appsettings.json");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                
                .UseSkiaSharp()
                
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                return new HttpClient() { BaseAddress = new Uri("http://10.0.2.2:5217/") };
            });

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<TripService>();
            builder.Services.AddSingleton<StopService>();
            builder.Services.AddSingleton<SuggestService>();
            builder.Services.AddSingleton<PdfService>();
            builder.Services.AddSingleton<ReviewService>();
            builder.Services.AddSingleton<GeoService>();
            builder.Services.AddSingleton<WeatherService>();
            builder.Services.AddSingleton<RecommendationService>();
            
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
