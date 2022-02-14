using System;
using System.Reactive;
using System.Reactive.Disposables;

using Microsoft.Extensions.Logging;

using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class ManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, IScreen {

    private readonly ILogger<ManagerViewModel> _logger;

    private readonly IResolver _resolver;

    private string _caption;

    private IRoutableViewModel? _currentViewModel;

    public ManagerViewModel(ILogger<ManagerViewModel> logger, IScreen hostScreen, IResolver resolver) {
        _logger = logger;
        HostScreen = hostScreen;
        //Router = routingState;
        _resolver = resolver;

        ViewAudioManager = ReactiveCommand.Create(() => {
            var vm = _resolver.GetService<AudioRecordingsManagerViewModel>();
            Router.Navigate.Execute(vm);
        });

        ViewDisposition = ReactiveCommand.Create(() => {
            var vm = _resolver.GetService<DispositionViewModel>();
            Router.Navigate.Execute(vm);
        });

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