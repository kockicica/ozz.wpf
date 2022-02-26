using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateDispositionsWindow : DialogWindowBase<DialogResultBase> {

    CalendarDatePicker dp;

    public CreateDispositionsWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        dp = this.FindControl<CalendarDatePicker>("DatePicker");

        this.WhenActivated(d => { dp.Focus(); });


    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        dp.Focus();
    }

    protected override void OnTemplateApplied(TemplateAppliedEventArgs e) {
        base.OnTemplateApplied(e);
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}