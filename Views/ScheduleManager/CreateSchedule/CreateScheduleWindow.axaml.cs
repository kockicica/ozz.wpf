using Avalonia;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateScheduleWindow : DialogWindowBase<CreateScheduleWindowResult> {
    public CreateScheduleWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}