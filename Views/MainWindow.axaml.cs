using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using LibVLCSharp.Shared;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ShowOverlay() {
            ViewModel!.OverlayVisible = true;
        }
        
        public void HideOverlay() {
            ViewModel!.OverlayVisible = false;
        }
    }
}