using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using LibVLCSharp.Avalonia;

using ozz.wpf.Dialog;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class ModalAudioPlayerWindow : DialogWindowBase<DialogResultBase> {

    public ModalAudioPlayerWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();

#endif
        this.WhenActivated(d => {
            if (DataContext is ModalAudioPlayerViewModel vm) {
                if (vm.AutoPlay) {
                    vm.PlayerModel.Play.Execute(null);
                }
            }
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }


}