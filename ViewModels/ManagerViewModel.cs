using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class ManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, IScreen {

    private readonly ILogger<ManagerViewModel> _logger;

    private readonly IResolver _resolver;

    private AudioRecordingsManagerViewModel? _audioRecordingsManagerViewModel;

    private string _caption;

    private IRoutableViewModel? _currentViewModel;

    private DispositionViewModel? _dispositionViewModel;

    public ManagerViewModel(ILogger<ManagerViewModel> logger, IScreen hostScreen, IResolver resolver) {
        _logger = logger;
        HostScreen = hostScreen;
        //Router = routingState;
        _resolver = resolver;

        ViewAudioManager = ReactiveCommand.Create(
            () => {
                //var vm = _resolver.GetService<AudioRecordingsManagerViewModel>();
                Router.Navigate.Execute(AudioRecordingsManagerViewModel);
            },
            this.WhenAny(model => model.CurrentViewModel, x => x.Value?.UrlPathSegment != "audio-manager")
        );

        ViewDisposition = ReactiveCommand.Create(
            () => {
                //var vm = _resolver.GetService<DispositionViewModel>();
                Router.Navigate.Execute(DispositionViewModel);
            },
            this.WhenAny(model => model.CurrentViewModel, x => x.Value?.UrlPathSegment != "disposition")
        );

        GoBack = ReactiveCommand.CreateFromObservable(() => Router.NavigateBack.Execute());

        this.WhenActivated(d => {
            Router.CurrentViewModel
                  .Subscribe(model => {
                      CurrentViewModel = model;
                      if (model is ICaption cm) {
                          Caption = cm.Caption;
                      }
                      else {
                          Caption = "";
                      }
                      this.RaisePropertyChanged(nameof(HasCurrentModel));
                  })
                  .DisposeWith(d);
        });

    }

    public ReactiveCommand<Unit, Unit> ViewAudioManager { get; }

    public ReactiveCommand<Unit, Unit> ViewDisposition { get; }

    public string Caption {
        get => _caption;
        set => this.RaiseAndSetIfChanged(ref _caption, value);
    }

    public ReactiveCommand<Unit, IRoutableViewModel?> GoBack { get; }

    public IRoutableViewModel? CurrentViewModel {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public bool HasCurrentModel => CurrentViewModel != null;

    public AudioRecordingsManagerViewModel AudioRecordingsManagerViewModel
        => _audioRecordingsManagerViewModel ??= _resolver.GetService<AudioRecordingsManagerViewModel>();

    public DispositionViewModel DispositionViewModel => _dispositionViewModel ??= _resolver.GetService<DispositionViewModel>();

    public IEnumerable<ManagerMenuItem> MenuItems => new ManagerMenuItem[] {
        new() { Caption = "Emitovanje zapisa", Command = ViewDisposition, Icon = "/Assets/circle-play.svg" },
        new() { Caption = "Upravljanje audio zapisima", Command = ViewAudioManager, Icon = "/Assets/file-audio.svg" },
    };

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "manager";
    public IScreen HostScreen     { get; }

    #endregion

    #region IScreen Members

    public RoutingState Router { get; } = new();

    #endregion

}

public class ManagerMenuItem {
    public ICommand Command { get; set; }
    public string   Caption { get; set; }
    public string   Icon    { get; set; }
}