using System;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.Models;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    private IDisposable d;

    public DispositionView() {
        InitializeComponent();

        this.WhenActivated(d => {
            ViewModel?.HandleSelectedDisposition.Subscribe(unit => {
                InputManager.Instance.ProcessInput(
                    new RawKeyEventArgs(KeyboardDevice.Instance,
                                        0,
                                        null,
                                        RawKeyEventType.KeyDown,
                                        Key.Down,
                                        RawInputModifiers.None));
            }).DisposeWith(d);
        });

    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnKeyUp(object? sender, KeyEventArgs e) {
        if (e.KeyModifiers == KeyModifiers.Alt) {
            Category newCat;
            var cats = ViewModel!.Categories.ToList();
            int offset = e.Key - Key.D1;
            if (cats.Count > offset) {
                ViewModel!.SelectedCategory = cats[offset];
            }
        }
    }
}