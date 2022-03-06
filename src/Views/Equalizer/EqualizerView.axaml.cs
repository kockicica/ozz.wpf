using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.Equalizer;

public partial class EqualizerView : ReactiveUserControl<EqualizerViewModel> {

    public EqualizerView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}