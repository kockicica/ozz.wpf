using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using LibVLCSharp.Shared;

namespace ozz.wpf.Views
{
    public partial class MainWindow : Window
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
            Core.Initialize();
        }
    }
}