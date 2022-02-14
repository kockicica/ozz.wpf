using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views;

public partial class AudioRecordingsManagerView : ReactiveUserControl<AudioRecordingsManagerViewModel> {
    public AudioRecordingsManagerView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}