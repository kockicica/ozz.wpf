using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    public DispositionView() {
        InitializeComponent();

        this.WhenActivated(disposable => {
            var mdl = this.ViewModel;
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

}