using System;
using System.Net.Http;
using System.Reflection;

using Avalonia;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;
using ozz.wpf.Views;

using ReactiveUI;

using Serilog;
using Serilog.Events;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;

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
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                         .WriteTo.Console()
                         .WriteTo.File("log-.log", rollingInterval: RollingInterval.Day)
                         .CreateBootstrapLogger();
            //.CreateLogger();


            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => { Log.Logger.Error("exc: {@e}", eventArgs.ExceptionObject); };
            var host = Host.CreateDefaultBuilder()
                           // .UseSerilog((context, services, configuration) => {
                           //     configuration.ReadFrom.Services(services)
                           //                  .Enrich.FromLogContext()
                           //                  .WriteTo.Console()
                           //                  .WriteTo.File("log-.log", rollingInterval: RollingInterval.Day);
                           // })
                           .ConfigureServices(
                               s => {
                                   s.UseMicrosoftDependencyResolver();
                                   var resolver = Locator.CurrentMutable;
                                   resolver.InitializeSplat();
                                   resolver.InitializeReactiveUI();

                                   // s.AddHttpClient(client => {
                                   //     client.BaseAddress = new Uri("http://localhost:27000");
                                   // });
                                   //s.AddHttpClient("default", client => { client.BaseAddress = new Uri("http://localhost:27000"); });

                                   //s.AddSingleton<IDataService, DataService>();
                                   s.AddSingleton<IEqualizerPresetFactory, VLCEqualizePresetFactory>();

                                   s.AddSingleton<MainWindowViewModel>();
                                   s.AddSingleton<DispositionViewModel>();
                                   s.AddTransient<AudioPlayerViewModel>();
                                   s.AddTransient<ModalAudioPlayerViewModel>();

                                   s.AddHttpClient<IDataService, DataService>(client => client.BaseAddress = new Uri("http://localhost:27000"));
                               })
                           .ConfigureLogging(builder => {
                               builder.AddSerilog();
                               builder.AddSplat();
                           })
                           .UseEnvironment(Environments.Development)
                           .Build();

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