using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views;

public partial class AudioPlayerWindow : ReactiveWindow<AudioPlayerViewModel> {

    public AudioPlayerWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        Close();
    }

}