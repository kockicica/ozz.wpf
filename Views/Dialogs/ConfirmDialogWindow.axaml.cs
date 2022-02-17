using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;
using ozz.wpf.ViewModels.Dialogs;

namespace ozz.wpf.Views.Dialogs;

public partial class ConfirmDialogWindow : DialogWindowBase<ConfirmDialogResult> {
    public ConfirmDialogWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}