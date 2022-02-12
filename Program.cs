using System;
using System.Net.Http;
using System.Reflection;

using Avalonia;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ozz.wpf.Dialog;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;
using ozz.wpf.Views;

using ReactiveUI;

using Serilog;

using Splat;

using ILogger = Serilog.ILogger;

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
                         .WriteTo.File("log-.log", rollingInterval: RollingInterval.Day)
                         .CreateLogger();


            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Log.Logger.Error("exc: {@e}", eventArgs.ExceptionObject); };
            var host = new HostBuilder().ConfigureServices(
                                            s => {
                                                s.AddHttpClient("default", client => { client.BaseAddress = new Uri("http://localhost:27000"); });
                                                //s.AddLogging(builder => builder.AddSerilog());
                                            })
                                        .Build();

            Locator.CurrentMutable.Register<IDataService>(
                () => new DataService(Locator.Current.GetService<IHttpClientFactory>(), Locator.Current.GetService<ILogger>()));
            Locator.CurrentMutable.Register(
                () => new DispositionViewModel(Locator.Current.GetService<IDataService>(),
                                               Locator.Current.GetService<ILogger>(),
                                               Locator.Current.GetService<IEqualizerPresetFactory>()
                )
            );
            Locator.CurrentMutable.Register<IHttpClientFactory>(() => host.Services.GetService<IHttpClientFactory>());
            Locator.CurrentMutable.Register(() => Log.Logger);
            //Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            //Locator.CurrentMutable.Register<IViewFor<DispositionViewModel>>(() => new MainWindow);
            //Locator.CurrentMutable.Register<IViewFor<AudioRecordingsListViewModel>>(() => new AudioRecordingsListViewModel());
            Locator.CurrentMutable.Register(() => new AudioPlayerViewModel(Locator.Current.GetService<ILogger>()));
            Locator.CurrentMutable.Register(() => new ModalAudioPlayerViewModel(Locator.Current.GetService<ILogger>()));
            Locator.CurrentMutable.Register<IEqualizerPresetFactory>(() => new VLCEqualizePresetFactory());
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

            Log.CloseAndFlush();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                         .UsePlatformDetect()
                         .LogToTrace()
                         .UseReactiveUI();

    }

}