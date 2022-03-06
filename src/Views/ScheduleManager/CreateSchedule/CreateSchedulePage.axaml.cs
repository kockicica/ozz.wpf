using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateSchedulePage : ReactiveUserControl<CreateSchedulePageViewModel> {
    public CreateSchedulePage() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}