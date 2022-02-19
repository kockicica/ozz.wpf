using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.AudioManager;

public partial class AudioRecordingDetailsView : ReactiveUserControl<AudioRecordingDetailsViewModel> {

    public AudioRecordingDetailsView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}