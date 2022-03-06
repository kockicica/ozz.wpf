using System.Reactive;

using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

using ReactiveUI;

namespace ozz.wpf.Views.Dialogs;

public partial class ModalAudioPlayerWindow : DialogWindowBase<DialogResultBase> {

    public ModalAudioPlayerWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();

#endif
        this.WhenActivated(d => {
            if (DataContext is ModalAudioPlayerViewModel vm) {
                if (vm.AutoPlay) {
                    vm.PlayerModel.Play.Execute(Unit.Default);
                }
            }
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}