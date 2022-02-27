using System;

using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateDispositionsWindow : DialogWindowBase<DialogResultBase> {


    public CreateDispositionsWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif




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