using System;
using System.Reflection;

using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.ViewModels;
using ozz.wpf.Views;
using ozz.wpf.Views.AudioManager;
using ozz.wpf.Views.Dialogs;
using ozz.wpf.Views.Disposition;
using ozz.wpf.Views.Manager;
using ozz.wpf.Views.Player;
using ozz.wpf.Views.ScheduleManager;
using ozz.wpf.Views.ScheduleManager.CreateSchedule;

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
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                         .WriteTo.Console()
                         .WriteTo.File("log-.log", rollingInterval: RollingInterval.Day)
                         //.CreateBootstrapLogger();
                         .CreateLogger();


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
                               //s.AddSingleton<IEqualizerPresetFactory, VLCEqualizePresetFactory>();
                               s.AddSingleton<IEqualizerPresetFactory, RemoteEqualizePresetFactory>();
                               s.AddSingleton<IResolver, LocatorBasedResolver>();

                               s.AddSingleton<MainWindowViewModel>();
                               s.AddSingleton<IScreen, MainWindowViewModel>(provider => provider.GetRequiredService<MainWindowViewModel>());

                               s.AddTransient<DispositionViewModel>();
                               s.AddTransient<AudioPlayerViewModel>();
                               s.AddTransient<LoginViewModel>();
                               s.AddTransient<ModalAudioPlayerViewModel>();
                               s.AddTransient<IViewFor<DispositionViewModel>, DispositionView>();

                               s.AddTransient<AudioRecordingsManagerViewModel>();
                               s.AddTransient<IViewFor<AudioRecordingsManagerViewModel>, AudioRecordingsManagerView>();

                               s.AddTransient<ManagerViewModel>();
                               s.AddTransient<IViewFor<ManagerViewModel>, ManagerView>();
                               s.AddTransient<AudioRecordingDetailsViewModel>();
                               s.AddTransient<IViewFor<AudioRecordingDetailsViewModel>, AudioRecordingDetailsView>();
                               s.AddTransient<EditAudioRecordingViewModel>();
                               s.AddTransient<ConfirmDialogViewModel>();
                               s.AddTransient<ScheduleManagerViewModel>();
                               s.AddTransient<IViewFor<ScheduleManagerViewModel>, ScheduleManagerView>();
                               s.AddTransient<CreateScheduleViewModel>();
                               s.AddTransient<IViewFor<CreateScheduleViewModel>, CreateScheduleView>();
                               s.AddTransient<CreateSchedulePageViewModel>();
                               s.AddTransient<IViewFor<CreateSchedulePageViewModel>, CreateSchedulePage>();
                               s.AddTransient<ScheduleRecordingViewModel>();
                               s.AddTransient<IViewFor<ScheduleRecordingViewModel>, ScheduleRecordingView>();
                               s.AddTransient<CreateScheduleWindowViewModel>();
                               s.AddTransient<CreateDispositionViewModel>();
                               s.AddTransient<DispositionSelectViewModel>();
                               s.AddTransient<DispositionBlockViewModel>();
                               s.AddTransient<AudioRecordingsLogViewModel>();
                               s.AddTransient<IViewFor<AudioRecordingsLogViewModel>, AudioRecordingsLogView>();

                               s.AddSingleton<RoutingState>(provider => {
                                   var wnd = provider.GetService<MainWindowViewModel>();
                                   return wnd.Router;
                               });

                               s.AddSingleton<INotificationManager>(provider => {
                                   var vm = provider.GetService<MainWindowViewModel>();
                                   return vm.NotificationManager;
                               });

                               s.AddSingleton<IOzzInteractions, OzzInteractions>();

                               s.AddSingleton<IMainWindowProvider, MainWindowProvider>();
                               s.AddSingleton<IAppStateManager, AppStateManager>();

                               // configure settings
                               s.AddOptions<ServerConfiguration>()
                                .Bind(ctx.Configuration.GetSection(ServerConfiguration.Server))
                                .PostConfigure(configuration => {
                                    if (string.IsNullOrEmpty(configuration.Url)) {
                                        configuration.Url = "http://localhost:27000";
                                    }
                                });

                               s.AddOptions<AudioPlayerConfiguration>()
                                .Bind(ctx.Configuration.GetSection(AudioPlayerConfiguration.AudioPlayer));

                               s.AddHttpClient<IClient, Client>((services, client) => {
                                   var config = services.GetService<IOptions<ServerConfiguration>>();
                                   client.BaseAddress = new Uri(config.Value.Url);

                               });

                               s.AddHttpClient<IScheduleClient, ScheduleClient>((services, client) => {
                                   var config = services.GetService<IOptions<ServerConfiguration>>();
                                   client.BaseAddress = new Uri(config.Value.Url);

                               });

                               s.AddHttpClient<IAudioRecordingsService, BackendAudioRecordingsService>((services, client) => {
                                   var config = services.GetService<IOptions<ServerConfiguration>>();
                                   client.BaseAddress = new Uri(config.Value.Url);
                               });

                               // add automapper
                               s.AddAutoMapper(expression => expression.AddMaps(Assembly.GetExecutingAssembly()));

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