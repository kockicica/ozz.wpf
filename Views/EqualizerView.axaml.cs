using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views;

public partial class EqualizerView : ReactiveUserControl<EqualizerViewModel> {

    public EqualizerView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}