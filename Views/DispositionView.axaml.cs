using System;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.Models;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    private IDisposable d;

    public DispositionView() {
        InitializeComponent();

        this.WhenActivated(disposables => disposables(ViewModel!.ShowPlayer.RegisterHandler(DoShowDialog)));
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private Task DoShowDialogAsync(InteractionContext<AudioRecording, Unit> interactionContext) {
        var d = interactionContext.Input;
        interactionContext.SetOutput(Unit.Default);
        return Task.CompletedTask;
    }

    private void DoShowDialog(InteractionContext<AudioRecording, Unit> interactionContext) {
        var d = interactionContext.Input;
        interactionContext.SetOutput(Unit.Default);
    }

}