using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.ViewModels;
using ozz.wpf.Views.AudioManager;
using ozz.wpf.Views.ScheduleManager;
using ozz.wpf.Views.ScheduleManager.CreateSchedule;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.Views.Manager;

public class ManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, IScreen {

    private readonly ILogger<ManagerViewModel> _logger;

    private readonly INotificationManager _notificationManager;

    private readonly IOzzInteractions _ozzInteractions;

    private readonly IResolver _resolver;

    private AudioRecordingsLogViewModel? _audioRecordingsLogViewModel;

    private AudioRecordingsManagerViewModel? _audioRecordingsManagerViewModel;

    private string _caption = "Home";

    private AudioRecordingDetailsViewModel? _createAudioRecordingViewModel;

    private CreateScheduleViewModel? _createScheduleViewModel;

    private IRoutableViewModel? _currentViewModel;

    private DispositionViewModel? _dispositionViewModel;

    private ScheduleManagerViewModel? _scheduleManagerViewModel;

    private ScheduleManagerViewModel? _scheduleReportViewModel;

    public ManagerViewModel(ILogger<ManagerViewModel> logger, IScreen hostScreen, IResolver resolver, IOzzInteractions ozzInteractions,
                            INotificationManager notificationManager) {
        _logger = logger;
        HostScreen = hostScreen;
        //Router = routingState;
        _resolver = resolver;
        _ozzInteractions = ozzInteractions;
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

        ViewScheduleManager = ReactiveCommand.Create(
            () => { Router.Navigate.Execute(ScheduleManagerViewModel); }
        );

        // CreateSchedulePage = ReactiveCommand.Create(
        //     () => { Router.NavigateAndReset.Execute(CreateScheduleViewModel); }
        // );
        CreateSchedulePage =
            ReactiveCommand.CreateFromTask<Unit, IEnumerable<Schedule>?>(async (unit, token)
                                                                             => await _ozzInteractions.CreateSchedules.Handle(Unit.Default));

        CreateNewAudio =
            ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(async unit => await _ozzInteractions.CreateAudioRecording.Handle(Unit.Default));

        GoBack = ReactiveCommand.CreateFromObservable(() => Router.NavigateBack.Execute());

        CreateDispositions = ReactiveCommand.CreateFromTask(async () => { await _ozzInteractions.CreateDispositions.Handle(Unit.Default); });

        ViewScheduleReport = ReactiveCommand.Create(() => { Router.Navigate.Execute(ScheduleReportViewModel); });

        ViewAudioRecordingsLog = ReactiveCommand.Create(() => { Router.Navigate.Execute(AudioRecordingsLogViewModel); });

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
                    _notificationManager.Show(new Notification("Obave??tenje", "Audio zapis je uspe??no kreiran", NotificationType.Success));

                })
                .DisposeWith(d);

            CreateNewAudio
                .ThrownExceptions
                .Where(exception => exception is AudioRecordingCreateException)
                .Subscribe(exception => {
                    _notificationManager.Show(new Notification("Gre??ka",
                                                               $"Problem prilikom kreiranja audio zapisa:\n{exception.Message}",
                                                               NotificationType.Error));
                })
                .DisposeWith(d);

            CreateDispositions
                .Subscribe(unit => { })
                .DisposeWith(d);


        });

    }

    public ReactiveCommand<Unit, Unit> ViewAudioManager { get; }

    public ReactiveCommand<Unit, Unit> ViewDisposition { get; }

    public ReactiveCommand<Unit, AudioRecording?> CreateNewAudio { get; }

    public ReactiveCommand<Unit, Unit> ViewScheduleManager { get; }

    public ReactiveCommand<Unit, Unit> CreateDispositions { get; }

    public ReactiveCommand<Unit, Unit> ViewScheduleReport { get; }

    public ReactiveCommand<Unit, Unit> ViewAudioRecordingsLog { get; }


    //public ReactiveCommand<Unit, Unit> CreateSchedulePage { get; }
    public ReactiveCommand<Unit, IEnumerable<Schedule>?> CreateSchedulePage { get; }

    public string Caption {
        get => _caption;
        set => this.RaiseAndSetIfChanged(ref _caption, value);
    }

    public ReactiveCommand<Unit, Unit> GoBack { get; }

    public IRoutableViewModel? CurrentViewModel {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public bool HasCurrentModel => CurrentViewModel != null;

    public AudioRecordingsManagerViewModel AudioRecordingsManagerViewModel
        => _audioRecordingsManagerViewModel ??= _resolver.GetService<AudioRecordingsManagerViewModel>();

    public DispositionViewModel DispositionViewModel => _dispositionViewModel ??= _resolver.GetService<DispositionViewModel>();

    public IEnumerable<ManagerMenuItem> MenuItems => new ManagerMenuItem[] {
        new() { Caption = "Emitovanje zapisa", Command = ViewDisposition, Icon = "circle_play" },
        new() { Caption = "Upravljanje audio zapisima", Command = ViewAudioManager, Icon = "album_collection" },
        new() { Caption = "Novi audio zapis", Command = CreateNewAudio, Icon = "album_collection_circle_plus" },
        new() { Caption = "Upravljanje rasporedom", Command = ViewScheduleManager, Icon = "calendar_pen" },
        new() { Caption = "Novi raspored", Command = CreateSchedulePage, Icon = "calendar_plus" },
        new() { Caption = "Kreiranje dispozicija", Command = CreateDispositions, Icon = "person_ski_jumping" },
        new() { Caption = "Pregled rasporeda", Command = ViewScheduleReport, Icon = "calendar_range" },
        new() { Caption = "Log emitovanja", Command = ViewAudioRecordingsLog, Icon = "eyes" },
    };

    public AudioRecordingDetailsViewModel? CreateAudioRecordingViewModel
        => _resolver.GetService<AudioRecordingDetailsViewModel>();

    public ScheduleManagerViewModel? ScheduleManagerViewModel {
        get => _scheduleManagerViewModel ??= _resolver.GetService<ScheduleManagerViewModel>();
    }

    public CreateScheduleViewModel? CreateScheduleViewModel {
        get => _createScheduleViewModel ??= _resolver.GetService<CreateScheduleViewModel>();
    }

    public ScheduleManagerViewModel? ScheduleReportViewModel {
        get {
            if (_scheduleReportViewModel != null) {
                return _scheduleReportViewModel;
            }
            _scheduleReportViewModel = _resolver.GetService<ScheduleManagerViewModel>();
            _scheduleReportViewModel.IsReport = true;
            return _scheduleReportViewModel;
        }
    }

    public AudioRecordingsLogViewModel? AudioRecordingsLogViewModel
        => _audioRecordingsLogViewModel ??= _resolver.GetService<AudioRecordingsLogViewModel>();

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

    public Geometry Geometry {
        get {
            if (App.Current.TryFindResource(Icon, out object? g)) {
                if (g is Geometry gg) {
                    return gg;
                }
            }
            return null;
        }
    }
}