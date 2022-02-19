using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager;

public partial class ScheduleManagerView : ReactiveUserControl<ScheduleManagerViewModel> {
    public ScheduleManagerView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}