using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Views {
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel> {
        Button _closeButton;

        WindowNotificationManager _notificationManager;

        public MainWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _notificationManager = new WindowNotificationManager(this);
            _closeButton = this.FindControl<Button>("CloseButton");

            this.WhenActivated(d => {
                ViewModel!.NotificationManager = _notificationManager;
                //Observable.FromEventPattern<RoutedEventArgs>(_closeButton, "Click").Subscribe(pattern => this.Close()).DisposeWith(d);
            });
        }

        public void ShowOverlay() {
            ViewModel!.OverlayVisible = true;
        }

        public void HideOverlay() {
            ViewModel!.OverlayVisible = false;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}