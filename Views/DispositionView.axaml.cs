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

    private async Task DoShowDialogAsync(InteractionContext<AudioRecording, Unit> interactionContext) {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            if (desktop.MainWindow is MainWindow wnd) {
                var modal = new AudioPlayerWindow {
                    DataContext = new AudioPlayerViewModel { Track = interactionContext.Input }
                };
                wnd.ShowOverlay();
                await modal.ShowDialog(wnd);
                wnd.HideOverlay();

            }

        }

        interactionContext.SetOutput(Unit.Default);

        //return Task.CompletedTask;
    }

}