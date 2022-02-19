using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;
using ozz.wpf.Views.Player;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.Views.ScheduleManager;

public class ScheduleManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel {
    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly ILogger<ScheduleManagerViewModel> _logger;
    private readonly INotificationManager              _notificationManager;

    private readonly AudioPlayerViewModel _playerViewModel;

    private readonly IScheduleClient _scheduleClient;
    private          DateTime?       _fromDate;

    private ObservableCollection<Schedule> _schedules = new();
    private AudioRecording?                _selectedRecording;
    private DateTime?                      _toDate;

    public ScheduleManagerViewModel(ILogger<ScheduleManagerViewModel> logger, IScreen hostScreen, IScheduleClient scheduleClient,
                                    IAudioRecordingsService audioRecordingsService, AudioPlayerViewModel playerViewModel,
                                    INotificationManager notificationManager) {
        _logger = logger;
        HostScreen = hostScreen;
        _scheduleClient = scheduleClient;
        _audioRecordingsService = audioRecordingsService;
        _playerViewModel = playerViewModel;
        _notificationManager = notificationManager;

        PlaySample = ReactiveCommand.Create<AudioRecording>(recording => {
            var rec = recording;
        });

        Search = ReactiveCommand.CreateFromTask<Unit, IEnumerable<Schedule>>(async (_, token) => {
            var sp = new ScheduleSearchParams { Recording = SelectedRecording?.Id, FromDate = FromDate, ToDate = ToDate };
            return await _scheduleClient.Find(sp, token);
        });


        this.WhenActivated(d => {
            Search
                .Catch(Observable.Return(new Schedule[] { }))
                .Subscribe(schedules => { Schedules = new ObservableCollection<Schedule>(schedules); })
                .DisposeWith(d);

            Search
                .ThrownExceptions
                //.Where(exception => exception is DatabaseException)
                .Subscribe(exception => {
                    _notificationManager.Show(new Notification("Greška", $"Greška prilikom pretrage:\n{exception.Message}", NotificationType.Error));
                })
                .DisposeWith(d);
        });

    }

    public ObservableCollection<Schedule> Schedules {
        get => _schedules;
        set => this.RaiseAndSetIfChanged(ref _schedules, value);
    }

    public DateTime? FromDate {
        get => _fromDate;
        set => this.RaiseAndSetIfChanged(ref _fromDate, value);
    }

    public DateTime? ToDate {
        get => _toDate;
        set => this.RaiseAndSetIfChanged(ref _toDate, value);
    }


    public AudioRecording? SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public TimeSpan AutoCompleteThrottleTime { get; } = TimeSpan.FromMilliseconds(500);

    public ReactiveCommand<AudioRecording, Unit> PlaySample { get; }

    public ReactiveCommand<Unit, IEnumerable<Schedule>> Search { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; }
    public IScreen HostScreen     { get; }

    #endregion

    public async Task<IEnumerable<object>> PopulateAsync(string searchText, CancellationToken token) {
        var sp = new AudioRecordingsSearchParams {
            Name = searchText,
            Count = 200,
            Sort = "-Date",
        };
        var res = await _audioRecordingsService.AudioRecordings(sp, token);
        return res.Data.AsEnumerable();
    }
}