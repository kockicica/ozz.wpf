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

using Splat;

using ILogger = Serilog.ILogger;

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
                var vm = Locator.Current.GetService<AudioPlayerViewModel>();
                vm.Track = interactionContext.Input;
                //var modal = new DialogWindow() { DataContext = new DialogWindowViewModel(){Content = vm} };

                var modal = new ModalAudioPlayerWindow {
                    DataContext = vm
                };
                wnd.ShowOverlay();
                await modal.ShowDialog(wnd);
                //await Task.Delay(1000);
                wnd.HideOverlay();

            }

        }

        interactionContext.SetOutput(Unit.Default);

        //return Task.CompletedTask;
    }

}