using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using LibVLCSharp.Avalonia;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views;

public partial class ModalAudioPlayerWindow : DialogWindowBase<DialogResultBase> {

    VideoView _videoView;

    public ModalAudioPlayerWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();

#endif
        this._videoView = this.FindControl<VideoView>("VideoView");
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }


}