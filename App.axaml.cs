using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using LibVLCSharp.Shared;

using ozz.wpf.ViewModels;
using ozz.wpf.Views;

using Splat;

namespace ozz.wpf {

    public partial class App : Application {

        public override void Initialize() {
            Core.Initialize();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var vm = Locator.Current.GetService<MainWindowViewModel>();
                vm.Disposition = Locator.Current.GetService<DispositionViewModel>();
                vm.LoginViewModel = Locator.Current.GetService<LoginViewModel>();
                desktop.MainWindow = new MainWindow {
                    DataContext = vm,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }

}