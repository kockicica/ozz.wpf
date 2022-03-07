using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.AudioManager;

public partial class AudioRecordingsLogView : ReactiveUserControl<AudioRecordingsLogViewModel> {
    public AudioRecordingsLogView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}