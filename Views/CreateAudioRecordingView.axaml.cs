using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views;

public partial class CreateAudioRecordingView : ReactiveUserControl<CreateAudioRecordingViewModel> {

    public CreateAudioRecordingView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}