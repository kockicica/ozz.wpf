using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Collections;
using Avalonia.Controls.Notifications;

using DynamicData;
using DynamicData.Binding;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.Views.Disposition;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.ViewModels;

public class DispositionViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {

    private readonly IAudioRecordingsService        _audioRecordingsService;
    private readonly ILogger<DialogWindowViewModel> _logger;
    private readonly INotificationManager           _notificationManager;
    private readonly IOzzInteractions               _ozzInteractions;
    private readonly IScheduleClient                _scheduleClient;

    private ObservableAsPropertyHelper<IEnumerable<Category>?>      _categories;
    private DispositionSelectItem                                   _currentDisposition;
    private DispositionBlock                                        _dispositionBlock;
    private DataGridCollectionView                                  _dispositions;
    private ObservableAsPropertyHelper<IEnumerable<AudioRecording>> _recordings;
    private bool?                                                   _searchActive = true;
    private DateTime?                                               _searchFrom;
    private Subject<bool>                                           _searchSubject = new();
    private string                                                  _searchTerm;
    private DateTime?                                               _searchTo;
    private Category?                                               _selectedCategory;
    private Disposition?                                            _selectedDisposition;
    private AudioRecording?                                         _selectedRecording;

    public DispositionViewModel(IClient client, ILogger<DialogWindowViewModel> logger, IAudioRecordingsService audioRecordingsService, IScreen screen,
                                IOzzInteractions ozzInteractions, IScheduleClient scheduleClient, INotificationManager notificationManager) {

        _logger = logger;
        _audioRecordingsService = audioRecordingsService;
        HostScreen = screen;
        _ozzInteractions = ozzInteractions;
        _scheduleClient = scheduleClient;
        _notificationManager = notificationManager;
        _dispositionBlock = new DispositionBlock();

        ProcessCategory = ReactiveCommand.Create<Category>(cat => SelectedCategory = cat, Observable.Return(true));

        ClearDate = ReactiveCommand.Create<string?>(which => {
            switch (which) {
                case "searchFrom":
                    SearchFrom = null;
                    break;
                case "searchTo":
                    SearchTo = null;
                    break;
            }
        });

        ViewPlayerCommand = ReactiveCommand.Create<AudioRecording>(ExecuteShowPlayerInteraction);

        SelectDisposition = ReactiveCommand.CreateFromTask<Unit, DispositionSelectItem?>(async unit => {
            var res = await _ozzInteractions.SelectDisposition.Handle(Unit.Default);
            return res;
        });

        SortDispositionsByRemaining = ReactiveCommand.Create(() => {
            Dispositions?.SortDescriptions.Clear();
            Dispositions?.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(Disposition.PlayCountRemaining)).SwitchSortDirection());
            Dispositions?.Refresh();
            SelectedDisposition = (Disposition?)Dispositions?[0];
        });

        HandleSelectedDisposition = ReactiveCommand.Create(() => {
            if (SelectedDisposition != null) {
                DispositionBlock.HandleDisposition(SelectedDisposition);
                this.RaisePropertyChanged(nameof(CurrentBlockCount));
                this.RaisePropertyChanged(nameof(CurrentBlockDuration));
            }
        });

        ClearSelectedDispositions = ReactiveCommand.Create(() => {
            DispositionBlock.Clear();
            this.RaisePropertyChanged(nameof(CurrentBlockCount));
            this.RaisePropertyChanged(nameof(CurrentBlockDuration));
        });

        ShowBlockPlayer = ReactiveCommand.CreateFromTask(async token => {
            var res = await _ozzInteractions.ShowBlockPlayer.Handle(DispositionBlock);
        });

        this.WhenActivated(d => {

            _categories = client
                          .Categories()
                          .ToObservable()
                          .Do(categories => { SelectedCategory = categories.FirstOrDefault()!; })
                          .Catch(Observable.Return(new List<Category> { new() { Id = 1, Name = "Errr", Order = 1 } }))
                          .ToProperty(this, x => x.Categories).DisposeWith(d);


            _recordings = Observable
                          .Merge(
                              this.WhenAnyValue(model => model.SelectedCategory)
                                  .Throttle(TimeSpan.FromMilliseconds(100))
                                  .Where(x => x != null)
                                  .Select(x => true),
                              this.WhenAnyValue(x => x.SearchTerm).Throttle(TimeSpan.FromMilliseconds(500)).Skip(1).Select(_ => true),
                              this.WhenAnyValue(x => x.SearchActive, x => x.SearchFrom, x => x.SearchTo).Skip(1).Select(_ => true)
                          )
                          .SelectMany(
                              Observable
                                  .FromAsync(ExecuteAsyncSearch)
                                  .Catch(Observable.Return(new PagedResults<AudioRecording>()))
                          )
                          .Select(x => x.Data)
                          .ToProperty(this, x => x.Recordings).DisposeWith(d);

            // select first item on any recordings list update
            this.WhenAnyValue(model => model.Recordings)
                .Where(recordings => recordings != null)
                .Subscribe(recordings => SelectedRecording = recordings.FirstOrDefault())
                .DisposeWith(d);

            SelectDisposition
                .Where(x => x != null)
                .Do(x => CurrentDisposition = x)
                .SelectMany(item => _scheduleClient.FindDispositions(new DispositionSearchParams { Date = item.Date, Shift = item.Shift })
                                                   .ToObservable(RxApp.MainThreadScheduler)
                                                   .Catch<IEnumerable<Disposition>, Exception>(err => {
                                                       var msg = new Notification("Greška",
                                                                                  $"Greška prilikom preterage dispozicija:\r\n{err.Message}",
                                                                                  NotificationType.Error);
                                                       _notificationManager.Show(msg);
                                                       return Observable.Never<IEnumerable<Disposition>?>()!;
                                                   })
                )
                .Subscribe(dispositions => {
                    Dispositions = new DataGridCollectionView(dispositions, false, false);
                    DispositionBlock.Clear();
                    this.RaisePropertyChanged(nameof(CurrentBlockCount));
                    this.RaisePropertyChanged(nameof(CurrentBlockDuration));
                })
                .DisposeWith(d);


            DispositionBlock
                .Dispositions
                .ToObservableChangeSet()
                .WhenPropertyChanged(ds => ds.PlayCountCurrent, false)
                .SelectMany(value => _scheduleClient
                                     .MarkDispositionExecution(new DispositionExecuteParams {
                                             Schedule = value.Sender.ScheduleId,
                                             Shift = CurrentDisposition.Shift
                                         }
                                     )
                                     .ToObservable(RxApp.MainThreadScheduler)
                                     .Catch<Unit, Exception>(err => {
                                         var msg = new Notification("Greška",
                                                                    $"Greška prilikom markiranja dispozicije:\r\n{err.Message}",
                                                                    NotificationType.Error);
                                         _notificationManager.Show(msg);
                                         return Observable.Never<Unit>();
                                     }))
                .Subscribe()
                .DisposeWith(d);


        });

    }


    public IEnumerable<Category>? Categories {
        get => _categories?.Value;
    }

    public IEnumerable<AudioRecording> Recordings {
        get => _recordings?.Value;
    }

    public Category? SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public string SearchTerm {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    public AudioRecording? SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public ReactiveCommand<Category, Unit> ProcessCategory { get; }

    public ReactiveCommand<AudioRecording, Unit> ViewPlayerCommand { get; set; }

    public ReactiveCommand<string?, Unit> ClearDate { get; set; }

    public ReactiveCommand<Unit, DispositionSelectItem?> SelectDisposition { get; }

    public ReactiveCommand<Unit, Unit> SortDispositionsByRemaining { get; }

    public ReactiveCommand<Unit, Unit> HandleSelectedDisposition { get; }

    public ReactiveCommand<Unit, Unit> ClearSelectedDispositions { get; }

    public ReactiveCommand<Unit, Unit> ShowBlockPlayer { get; }

    public bool? SearchActive {
        get => _searchActive;
        set => this.RaiseAndSetIfChanged(ref _searchActive, value);
    }

    public DateTime? SearchFrom {
        get => _searchFrom;
        set => this.RaiseAndSetIfChanged(ref _searchFrom, value);
    }

    public DateTime? SearchTo {
        get => _searchTo;
        set => this.RaiseAndSetIfChanged(ref _searchTo, value);
    }

    public DispositionSelectItem CurrentDisposition {
        get => _currentDisposition;
        set => this.RaiseAndSetIfChanged(ref _currentDisposition, value);
    }

    public DataGridCollectionView Dispositions {
        get => _dispositions;
        set => this.RaiseAndSetIfChanged(ref _dispositions, value);
    }

    public Disposition? SelectedDisposition {
        get => _selectedDisposition;
        set => this.RaiseAndSetIfChanged(ref _selectedDisposition, value);
    }

    public DispositionBlock DispositionBlock {
        get => _dispositionBlock;
        set => this.RaiseAndSetIfChanged(ref _dispositionBlock, value);
    }

    public TimeSpan CurrentBlockDuration => DispositionBlock.TotalDuration;

    public int CurrentBlockCount => DispositionBlock.TotalCount;

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Emitovanje audio zapisa";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "disposition";
    public IScreen HostScreen     { get; }

    #endregion

    private async void ExecuteShowPlayerInteraction(AudioRecording recording) {
        await _ozzInteractions.ShowPlayer.Handle(recording);
    }

    private Task<PagedResults<AudioRecording>> ExecuteAsyncSearch(CancellationToken token) {

        var sp = new AudioRecordingsSearchParams {
            CategoryId = SelectedCategory?.Id,
            Name = SearchTerm,
            Active = SearchActive,
            FromDate = SearchFrom?.Date,
            ToDate = SearchTo?.Date,
            Skip = 0,
            Count = 1000,
            Sort = "-Date"
        };
        return _audioRecordingsService.AudioRecordings(sp, token);
    }
}