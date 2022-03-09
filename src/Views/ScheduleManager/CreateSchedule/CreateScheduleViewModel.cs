using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Collections;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateScheduleViewModel : ViewModelBase, IRoutableViewModel, IScreen, IActivatableViewModel, ICaption {

    private readonly IAudioRecordingsService _audioRecordingsService;
    private readonly IEqualizerPresetFactory _equalizerPresetFactory;

    private readonly ILogger<CreateScheduleViewModel> _logger;
    private readonly IScheduleClient                  _scheduleClient;

    private IRoutableViewModel?   _activeViewModel;
    private int                   _numberOfDays = 1;
    private AudioPlayerViewModel? _playerViewModel;

    private IResolver                   _resolver;
    private ScheduleRecordingViewModel? _scheduleRecordingViewModel;
    private AudioRecordingsSearchParams _searchParams = new() { Active = true };
    private AudioRecording?             _selectedRecording;
    private DateTime?                   _startDate = DateTime.Today;


    public CreateScheduleViewModel(ILogger<CreateScheduleViewModel> logger, IScreen hostScreen, IResolver resolver,
                                   IAudioRecordingsService audioRecordingsService, AudioPlayerViewModel? playerViewModel,
                                   ScheduleRecordingViewModel scheduleRecordingViewModel, IScheduleClient scheduleClient,
                                   IEqualizerPresetFactory equalizerPresetFactory) {
        _logger = logger;
        HostScreen = hostScreen;
        _resolver = resolver;
        _audioRecordingsService = audioRecordingsService;
        _playerViewModel = playerViewModel;
        _scheduleRecordingViewModel = scheduleRecordingViewModel;
        _scheduleClient = scheduleClient;
        _equalizerPresetFactory = equalizerPresetFactory;

        EditScheduleTable = ReactiveCommand.Create(() => { Router.Navigate.Execute(ScheduleRecordingViewModel!); }, CanEditScheduleTable);

        BackToGeneralData = ReactiveCommand.Create(() => { Router.NavigateBack.Execute(); });

        Create = ReactiveCommand.Create(() => {
            var schedules = ScheduleRecordingViewModel.Schedules;
        });

        this.WhenActivated(d => {

            Router.CurrentViewModel
                  .Subscribe(async model => {
                      _activeViewModel = model;
                      if (_activeViewModel == null) {
                          PlayerViewModel = _resolver.GetService<AudioPlayerViewModel>();
                          PlayerViewModel.Equalizer = await _equalizerPresetFactory.GetDefaultPreset();
                          if (SelectedRecording != null) {
                              PlayerViewModel.Track = SelectedRecording;
                          }
                      }
                  })
                  .DisposeWith(d);

            // when selected recording has been changed, update audio track data 
            this.WhenAnyValue(x => x.SelectedRecording)
                .Subscribe(recording => {
                    PlayerViewModel!.Track = recording;
                    ScheduleRecordingViewModel!.Recording = recording;
                })
                .DisposeWith(d);

            // recreate schedules when any of those values has been changed
            this.WhenAnyValue(x => x.SelectedRecording, x => x.StartDate, x => x.NumberOfDays)
                .Where(x => x.Item1 != null && x.Item2 != null)
                .Subscribe(tuple => {
                    var (audioRecording, startDate, numberOfDays) = tuple;
                    ScheduleRecordingViewModel!.Schedules = CreateSchedules(audioRecording!, startDate, numberOfDays);
                })
                .DisposeWith(d);
        });
    }

    public TimeSpan AutoCompleteThrottleTime { get; } = TimeSpan.FromMilliseconds(500);

    public AudioRecordingsSearchParams SearchParams {
        get => _searchParams;
        set => this.RaiseAndSetIfChanged(ref _searchParams, value);
    }

    public AudioRecording? SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public AudioPlayerViewModel? PlayerViewModel {
        get => _playerViewModel;
        set => this.RaiseAndSetIfChanged(ref _playerViewModel, value);
    }

    public DateTime? StartDate {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    public int NumberOfDays {
        get => _numberOfDays;
        set => this.RaiseAndSetIfChanged(ref _numberOfDays, value);
    }

    public ReactiveCommand<Unit, Unit> EditScheduleTable { get; }

    public ReactiveCommand<Unit, Unit> BackToGeneralData { get; }

    public ReactiveCommand<Unit, Unit> Create { get; }

    private IObservable<bool> CanEditScheduleTable
        => this.WhenAnyValue(x => x.SelectedRecording,
                             x => x.StartDate,
                             x => x.NumberOfDays,
                             (recording, time, days) => recording != null && time != null && _activeViewModel == null);

    private IObservable<bool> CanBackToGeneralData
        => Observable.Return(false).Merge(Router.CurrentViewModel.Select(x => x != null));

    public ScheduleRecordingViewModel? ScheduleRecordingViewModel
        => _scheduleRecordingViewModel ??= _resolver.GetService<ScheduleRecordingViewModel>();

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Novi raspored";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "create-schedule";
    public IScreen HostScreen     { get; }

    #endregion

    #region IScreen Members

    public RoutingState Router { get; } = new();

    #endregion

    public async Task<IEnumerable<object>> PopulateAsync(string searchText, CancellationToken token) {
        SearchParams.Name = searchText;
        var res = await _audioRecordingsService.AudioRecordings(SearchParams, token);
        return res.Data.AsEnumerable();
    }

    private DataGridCollectionView CreateSchedules(AudioRecording audioRecording, DateTime? startDate, int numberOfDays) {
        var schedules = Enumerable
                        .Range(0, numberOfDays)
                        .Select(i => new Schedule {
                            Date = startDate!.Value.AddDays(i),
                            Duration = TimeSpan.FromMilliseconds(audioRecording.Duration / 1_000_000),
                            Recording = SelectedRecording!,
                            Shift1 = 0,
                            Shift2 = 0,
                            Shift3 = 0,
                            Shift4 = 0,
                            TotalPlayCount = 0,
                        });
        return new DataGridCollectionView(schedules);
    }
}