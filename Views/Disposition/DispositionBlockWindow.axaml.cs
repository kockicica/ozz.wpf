using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views.Disposition;

public partial class DispositionBlockWindow : DialogWindowBase<DialogResultBase> {
    public DispositionBlockWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}