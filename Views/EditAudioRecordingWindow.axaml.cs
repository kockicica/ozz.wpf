using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;
using ozz.wpf.ViewModels;

namespace ozz.wpf.Views;

public partial class EditAudioRecordingWindow : DialogWindowBase<EditAudioRecordingsResult> {

    public EditAudioRecordingWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}