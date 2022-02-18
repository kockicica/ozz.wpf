using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;
using ozz.wpf.Services.Interactions.Confirm;
using ozz.wpf.ViewModels.Dialogs;

using ReactiveUI;

namespace ozz.wpf.Views.Dialogs;

public partial class ConfirmDialogWindow : DialogWindowBase<ConfirmDialogResult> {
    public ConfirmDialogWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        var yesButton = this.FindControl<Button>("YesButton");
        var noButton = this.FindControl<Button>("NoButton");
        this.WhenActivated(d => {
            if (ViewModel is ConfirmDialogViewModel vm) {
                var yesClasses = vm.ButtonTypes.Where(x => x.Button == ConfirmMessageResult.Yes).Select(c => c.Class);
                yesButton.Classes.AddRange(yesClasses);
                var noClasses = vm.ButtonTypes.Where(x => x.Button == ConfirmMessageResult.No).Select(c => c.Class);
                noButton.Classes.AddRange(noClasses);

            }
        });

    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}