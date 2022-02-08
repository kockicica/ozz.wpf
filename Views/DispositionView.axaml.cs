using System;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.Hosting.Internal;

using ozz.wpf.Models;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    private IDisposable d;

    public DispositionView() {
        InitializeComponent();

        this.WhenActivated(disposables => disposables(ViewModel!.ShowPlayer.RegisterHandler(DoShowDialogAsync)));
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private Task DoShowDialogAsync(InteractionContext<AudioRecording, Unit> interactionContext) {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var mainWindow = desktop.MainWindow;
            var modal = new AudioPlayerWindow();
            modal.DataContext = new AudioPlayerViewModel{Track = interactionContext.Input};
            modal.Show(mainWindow);

        }

        interactionContext.SetOutput(Unit.Default);
        return Task.CompletedTask;
    }

    private void DoShowDialog(InteractionContext<AudioRecording, Unit> interactionContext) {
        var d = interactionContext.Input;
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var wnd = desktop.MainWindow;
        }

        interactionContext.SetOutput(Unit.Default);
    }

}