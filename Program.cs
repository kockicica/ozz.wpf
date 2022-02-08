using System;
using System.Data.Common;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ozz.wpf.Services;
using ozz.wpf.ViewModels;

using ReactiveUI;

using Serilog;
using Serilog.Events;

using Splat;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ozz.wpf {

    class Program {

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {

            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .WriteTo.Console()
                         .WriteTo.File("log-.log", rollingInterval:RollingInterval.Day)
                         .CreateLogger();
            

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => {
                Console.WriteLine(eventArgs.ExceptionObject);
            };
            var host = new HostBuilder().ConfigureServices(s => {
                s.AddHttpClient("default", client => {
                    client.BaseAddress = new Uri("http://localhost:27000");
                });
                s.AddLogging(builder => {
                    builder.AddConsole();
                });
            }).Build();

            Locator.CurrentMutable.Register<IDataService>(() => new DataService(Locator.Current.GetService<IHttpClientFactory>()));
            Locator.CurrentMutable.Register(() => new DispositionViewModel(Locator.Current.GetService<IDataService>(), Locator.Current.GetService<ILogger>()));
            Locator.CurrentMutable.Register<IHttpClientFactory>(() => host.Services.GetService<IHttpClientFactory>());
            Locator.CurrentMutable.Register<ILogger>(() => host.Services.GetService<ILoggerProvider>().CreateLogger("ads"));
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToTrace()
                         .UseReactiveUI();

    }

}