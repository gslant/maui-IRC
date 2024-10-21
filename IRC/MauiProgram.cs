using IRC.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace IRC
{
    public static class MauiProgram
    {
        public static IServiceProvider Services { get; private set; }
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("JetBrainsMono-Medium.ttf", "JetBrainsMonoMedium");
                });

            builder.Services.AddTransient<Func<string, int, string, string, string, string?, ConnectionViewModel>>(serviceProvider => 
            (hostname, port, nickname, username, realname, password) => 
                new ConnectionViewModel(hostname, port, nickname, username, realname, password));

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            Services = app.Services;

            return app;
        }
    }

}
