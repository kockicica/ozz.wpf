using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.Manager;

public partial class ManagerView : ReactiveUserControl<ManagerViewModel> {
    public ManagerView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}