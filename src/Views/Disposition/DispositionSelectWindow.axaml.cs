using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using ozz.wpf.Dialog;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public partial class DispositionSelectWindow : DialogWindowBase<DispositionSelectResult> {

    ListBox list;

    public DispositionSelectWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        list = this.FindControl<ListBox>("ListBox");

        this.WhenActivated(d => {
            Observable
                .FromEventPattern<RoutedEventArgs>(list, nameof(ListBox.DoubleTapped))
                .SelectMany(_ => {
                    if (ViewModel is DispositionSelectViewModel vm) {
                        return vm.SelectDisposition.Execute();
                    }
                    return Observable.Never(Unit.Default);
                })
                .Subscribe()
                .DisposeWith(d);
        });
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