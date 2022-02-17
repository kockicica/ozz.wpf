using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.ViewModels;

public class ManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, IScreen {

    private readonly IBrowseForFile _browseForFile;

    private readonly ILogger<ManagerViewModel> _logger;

    private readonly INotificationManager _notificationManager;

    private readonly IResolver _resolver;

    private AudioRecordingsManagerViewModel? _audioRecordingsManagerViewModel;

    private string _caption = "Home";

    private AudioRecordingDetailsViewModel? _createAudioRecordingViewModel;

    private IRoutableViewModel? _currentViewModel;

    private DispositionViewModel? _dispositionViewModel;

    public ManagerViewModel(ILogger<ManagerViewModel> logger, IScreen hostScreen, IResolver resolver, IBrowseForFile browseForFile,
                            INotificationManager notificationManager) {
        _logger = logger;
        HostScreen = hostScreen;
        //Router = routingState;
        _resolver = resolver;
        _browseForFile = browseForFile;
        _notificationManager = notificationManager;

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

        // CreateNewAudio = ReactiveCommand.Create(
        //     () => {
        //         //var vm = _resolver.GetService<DispositionViewModel>();
        //         Router.Navigate.Execute(CreateAudioRecordingViewModel);
        //     },
        //     this.WhenAny(model => model.CurrentViewModel, x => x.Value?.UrlPathSegment != "create-record")
        // );
        CreateNewAudio =
            ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(async unit => await _browseForFile.CreateAudioRecording.Handle(Unit.Default));

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

            CreateNewAudio
                .Where(x => x != null)
                .Subscribe(recording => {
                    var a = recording;
                    _notificationManager.Show(new Notification("Obaveštenje", "Audio zapis je uspešno kreiran", NotificationType.Success));

                })
                .DisposeWith(d);

            CreateNewAudio
                .ThrownExceptions
                .Where(exception => exception is AudioRecordingCreateException)
                .Subscribe(exception => {
                    _notificationManager.Show(new Notification("Greška",
                                                               $"Problem prilikom kreiranja audio zapisa:\n{exception.Message}",
                                                               NotificationType.Error));
                })
                .DisposeWith(d);


        });

    }

    public ReactiveCommand<Unit, Unit> ViewAudioManager { get; }

    public ReactiveCommand<Unit, Unit> ViewDisposition { get; }

    public ReactiveCommand<Unit, AudioRecording?> CreateNewAudio { get; }

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
        new() { Caption = "Upravljanje audio zapisima", Command = ViewAudioManager, Icon = "/Assets/album-collection.svg" },
        new() { Caption = "Novi audio zapis", Command = CreateNewAudio, Icon = "/Assets/file-audio.svg" },
    };

    public AudioRecordingDetailsViewModel? CreateAudioRecordingViewModel
        => _resolver.GetService<AudioRecordingDetailsViewModel>();

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