using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views;

public partial class AudioPlayerWindow : DialogWindowBase<DialogResultBase> {

    public AudioPlayerWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }


}