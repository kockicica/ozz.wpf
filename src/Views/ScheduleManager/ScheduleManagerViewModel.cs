using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls.Notifications;

using DynamicData;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.Services.Interactions.Confirm;
using ozz.wpf.ViewModels;
using ozz.wpf.Views.Player;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.Views.ScheduleManager;

public class ScheduleManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {
    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly ILogger<ScheduleManagerViewModel> _logger;
    private readonly INotificationManager              _notificationManager;
    private readonly IOzzInteractions                  _ozzInteractions;
    private readonly AudioPlayerViewModel              _playerViewModel;
    private readonly IScheduleClient                   _scheduleClient;

    private DateTime?              _fromDate;
    private bool                   _isInEdit;
    private DataGridCollectionView _schedules      = new(Array.Empty<Schedule>());
    private SourceList<Schedule>   _scheduleSource = new();
    private AudioRecording?        _selectedRecording;
    private IList                  _selectedSchedules;
    private DateTime?              _toDate;

    public ScheduleManagerViewModel(ILogger<ScheduleManagerViewModel> logger, IScreen hostScreen, IScheduleClient scheduleClient,
                                    IAudioRecordingsService audioRecordingsService, AudioPlayerViewModel playerViewModel,
                                    INotificationManager notificationManager, IOzzInteractions ozzInteractions) {
        _logger = logger;
        HostScreen = hostScreen;
        _scheduleClient = scheduleClient;
        _audioRecordingsService = audioRecordingsService;
        _playerViewModel = playerViewModel;
        _notificationManager = notificationManager;
        _ozzInteractions = ozzInteractions;


        PlaySample = ReactiveCommand.Create<AudioRecording>(recording => {
            var rec = recording;
        });

        Search = ReactiveCommand.CreateFromTask<Unit, IEnumerable<Schedule>>(async (_, token) => {
            var sp = new ScheduleSearchParams { Recording = SelectedRecording?.Id, FromDate = FromDate, ToDate = ToDate };
            return await _scheduleClient.Find(sp, token);
        });

        HandleEnterKey = ReactiveCommand.Create(() => { });

        DeleteSchedules = ReactiveCommand.CreateFromTask<IList, IEnumerable<Schedule>?>(async (list, token) => {
            var msg = new ConfirmMessageConfig {
                Message = $"Da li ste sigurni da želite brisanje {list.Count} rasporeda?",
                ButtonTypes = new List<ConfirmButtonType> {
                    new() { Button = ConfirmMessageResult.Yes, Class = "danger" },
                    new() { Button = ConfirmMessageResult.No },
                }
            };
            var res = await _ozzInteractions.Confirm.Handle(msg);
            if (res != ConfirmMessageResult.Yes) return null;

            var schedules = list.Cast<Schedule>().ToList();
            foreach (var schedule in schedules) {
                await _scheduleClient.Delete(schedule.Id, token);
            }
            return schedules;
        });

        this.WhenActivated(d => {
            Search
                .Catch(Observable.Return(Array.Empty<Schedule>()))
                .Subscribe(schedules => {
                    var items = schedules.ToList();
                    Schedules = new DataGridCollectionView(items);
                    _scheduleSource.Edit(list => {
                        list.Clear();
                        list.AddRange(items);
                    });
                })
                .DisposeWith(d);

            Search
                .ThrownExceptions
                //.Where(exception => exception is DatabaseException)
                .Subscribe(exception => {
                    _notificationManager.Show(new Notification("Greška", $"Greška prilikom pretrage:\n{exception.Message}", NotificationType.Error));
                })
                .DisposeWith(d);

            this.WhenAnyValue(model => model.Schedules).Subscribe(view => {
                    var a = view;
                })
                .DisposeWith(d);

            //_scheduleSource.Connect().WhenAnyPropertyChanged().Subscribe(HandleChangedSchedule).DisposeWith(d);
            _scheduleSource
                .Connect()
                .WhenAnyPropertyChanged()
                .SelectMany(s => Observable.FromAsync(token => UpdateSchedule(s, token), RxApp.MainThreadScheduler))
                .Catch<Schedule?, Exception>(exception => {
                    _notificationManager.Show(new Notification("Greška", $"Greška prilikom promene:\n{exception.Message}", NotificationType.Error));
                    return Observable.Return<Schedule?>(null);
                })
                .Repeat()
                .Subscribe(schedule => {
                    var saved = schedule;
                })
                .DisposeWith(d);

            DeleteSchedules
                .Catch<IEnumerable<Schedule>?, Exception>(exception => {
                    _notificationManager.Show(new Notification("Greška", $"Greška prilikom brisanja:\n{exception.Message}", NotificationType.Error));
                    return Observable.Return<IEnumerable<Schedule>?>(null);
                })
                .Where(x => x != null)
                //.SelectMany(_ => Search.Execute())
                .Subscribe(schedules => {
                    foreach (var schedule in schedules) {
                        if (Schedules.Contains(schedule)) {
                            Schedules.Remove(schedule);
                        }
                    }
                })
                .DisposeWith(d);
        });

    }

    public DataGridCollectionView Schedules {
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

    public ReactiveCommand<Unit, Unit> HandleEnterKey { get; }

    public ReactiveCommand<IList, IEnumerable<Schedule>?> DeleteSchedules { get; }

    public bool IsInEdit {
        get => _isInEdit;
        set => this.RaiseAndSetIfChanged(ref _isInEdit, value);
    }

    public IList SelectedSchedules {
        get => _selectedSchedules;
        set => this.RaiseAndSetIfChanged(ref _selectedSchedules, value);
    }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Održavanje rasporeda";

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

    private void HandleChangedSchedule(Schedule? schedule) {
        var a = schedule;
    }

    private async Task<Schedule?> UpdateSchedule(Schedule? schedule, CancellationToken token) {
        if (schedule != null) {
            return await _scheduleClient.Update(schedule.Id, schedule);
        }
        return schedule;
    }
}