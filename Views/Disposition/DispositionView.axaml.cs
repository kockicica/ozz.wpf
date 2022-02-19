using System;

using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

namespace ozz.wpf.Views.Disposition;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    private IDisposable d;


    public DispositionView() {
        InitializeComponent();

    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}