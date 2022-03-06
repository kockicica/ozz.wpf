using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateScheduleWindow : DialogWindowBase<CreateScheduleWindowResult> {

    CreateScheduleView _createSchedule;

    public CreateScheduleWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        _createSchedule = this.FindControl<CreateScheduleView>("CreateScheduleView");

        // this.WhenActivated(d => {
        //     _createSchedule.TemplateApplied += (sender, args) => {
        //         _createSchedule.ViewHost.Router.NavigationChanged.Subscribe(set => {
        //             InputManager.Instance.ProcessInput(
        //                 new RawKeyEventArgs(KeyboardDevice.Instance,
        //                                     0,
        //                                     this,
        //                                     RawKeyEventType.KeyDown,
        //                                     Key.Tab,
        //                                     RawInputModifiers.None));
        //         }).DisposeWith(d);
        //     };
        // });
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        InputManager.Instance.ProcessInput(new RawKeyEventArgs(KeyboardDevice.Instance,
                                                               0,
                                                               this,
                                                               RawKeyEventType.KeyDown,
                                                               Key.Tab,
                                                               RawInputModifiers.None));
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}