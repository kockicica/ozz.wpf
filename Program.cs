﻿using System;

using Avalonia;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;

using ReactiveUI;

using Serilog;
using Serilog.Events;

using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using Splat.Microsoft.Extensions.Logging;

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
                           .ConfigureServices((ctx, s) => {
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
                               s.AddSingleton<IResolver, LocatorBasedResolver>();

                               s.AddSingleton<MainWindowViewModel>();
                               s.AddSingleton<DispositionViewModel>();
                               s.AddTransient<AudioPlayerViewModel>();
                               s.AddTransient<ModalAudioPlayerViewModel>();

                               // configure settings
                               s.AddOptions<ServerConfiguration>()
                                .Bind(ctx.Configuration.GetSection(ServerConfiguration.Server))
                                .PostConfigure(configuration => {
                                    if (string.IsNullOrEmpty(configuration.Url)) {
                                        configuration.Url = "http://localhost:27000";
                                    }
                                });

                               s.AddOptions<AudioPlayerConfiguration>()
                                .Bind(ctx.Configuration.GetSection(AudioPlayerConfiguration.AudioPlayer))
                                .PostConfigure(configuration => {
                                    configuration.Volume ??= 100;
                                    configuration.Volume = configuration.Volume switch {
                                        < 0 => 0,
                                        > 100 => 100,
                                        _ => configuration.Volume
                                    };
                                });

                               s.AddHttpClient<IDataService, DataService>((services, client) => {
                                   var config = services.GetService<IOptions<ServerConfiguration>>();
                                   client.BaseAddress = new Uri(config.Value.Url);

                               });

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