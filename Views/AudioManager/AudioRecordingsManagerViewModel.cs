using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using AutoMapper;

using Avalonia.Collections;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.Services.Interactions.Confirm;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {

    private readonly IAudioRecordingsService                  _audioRecordingsService;
    private readonly IClient                                  _client;
    private readonly ILogger<AudioRecordingsManagerViewModel> _logger;
    private readonly IMapper                                  _mapper;
    private readonly IOzzInteractions                         _ozzInteractions;

    private ObservableAsPropertyHelper<IEnumerable<Category?>> _categories;
    private int?                                               _currentPage;
    private bool                                               _isUpdate;
    private PagedResults<AudioRecording>?                      _pagedResults;
    private int                                                _pageSize     = 20;
    private DataGridCollectionView                             _results      = new(Array.Empty<AudioRecording>());
    private AudioRecordingsSearchParams                        _searchParams = new();
    private Category?                                          _selectedCategory;
    private AudioRecording?                                    _selectedRecording;
    private int                                                _totalRecords;


    public AudioRecordingsManagerViewModel(ILogger<AudioRecordingsManagerViewModel> logger, IScreen screen, IClient client,
                                           IAudioRecordingsService audioRecordingsService, IOzzInteractions ozzInteractions, IMapper mapper) {

        _logger = logger;
        HostScreen = screen;
        _client = client;
        _audioRecordingsService = audioRecordingsService;
        _ozzInteractions = ozzInteractions;
        _mapper = mapper;

        Search = ReactiveCommand.CreateFromTask<Unit, PagedResults<AudioRecording>>(
            (unit, token) => {
                SearchParams.Count = PageSize;
                SearchParams.Skip = (CurrentPage - 1) * PageSize;
                return _audioRecordingsService.AudioRecordings(SearchParams, token);
            });

        async Task<AudioRecording?> HandleEditRecording(AudioRecording recording) {
            var res = await _ozzInteractions.EditAudioRecording.Handle(recording);
            return res;
        }

        EditRecording = ReactiveCommand.CreateFromTask<AudioRecording, AudioRecording?>(HandleEditRecording);

        DeleteRecording = ReactiveCommand.CreateFromTask<AudioRecording, ConfirmMessageResult>(
            async recording => await _ozzInteractions.Confirm.Handle(new ConfirmMessageConfig {
                Message = $"Da li ste sigurni da želite da izbrišete {recording.Name}?", Title = "Pitanje"
            }));


        this.WhenActivated(d => {
            _categories = _client
                          .Categories().ToObservable()
                          .AddNull()
                          .ToProperty(this, x => x.Categories)
                          .DisposeWith(d);

            this.WhenAnyValue(model => model.SelectedCategory).Subscribe(category => { SearchParams.CategoryId = category?.Id; }).DisposeWith(d);

            Search
                .Subscribe(results => {
                    _pagedResults = results;
                    Results = new DataGridCollectionView(_pagedResults.Data);
                    this.RaisePropertyChanged(nameof(TotalRecords));
                })
                .DisposeWith(d);

            EditRecording
                .Where(recording => recording != null)
                .Subscribe(recording => {
                    if (Results.SourceCollection.Cast<AudioRecording>().SingleOrDefault(rec => rec.Id == recording.Id) is { } fnd) {
                        _mapper.Map(recording, fnd);
                    }
                })
                .DisposeWith(d);

            DeleteRecording
                .Where(x => x == ConfirmMessageResult.Yes)
                .SelectMany(_ => _audioRecordingsService.Delete(SelectedRecording!.Id).ToObservable())
                .SelectMany(_ => Search.Execute())
                .Subscribe()
                .DisposeWith(d);

            this.WhenAnyValue(x => x.PageSize)
                .Skip(1)
                .Subscribe(i => CurrentPage = 1)
                .DisposeWith(d);

            this.WhenAnyValue(x => x.CurrentPage)
                .Skip(1)
                .SelectMany(_ => Search.Execute())
                .Subscribe()
                .DisposeWith(d);

        });
    }

    public AudioRecordingsSearchParams SearchParams {
        get => _searchParams;
        set => this.RaiseAndSetIfChanged(ref _searchParams, value);
    }

    public IEnumerable<Category?> Categories {
        get {
            return _categories?.Value ?? new Category[] { };
        }
    }

    public DataGridCollectionView Results {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public ReactiveCommand<Unit, PagedResults<AudioRecording>> Search { get; set; }

    public ReactiveCommand<AudioRecording, AudioRecording?> EditRecording { get; set; }

    public ReactiveCommand<AudioRecording, ConfirmMessageResult> DeleteRecording { get; set; }

    public Category? SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public AudioRecording? SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public bool IsUpdate {
        get => _isUpdate;
        set => this.RaiseAndSetIfChanged(ref _isUpdate, value);
    }

    public int? CurrentPage {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public int PageSize {
        get => _pageSize;
        set => this.RaiseAndSetIfChanged(ref _pageSize, value);
    }

    // public int TotalRecords {
    //     get => _totalRecords;
    //     set => this.RaiseAndSetIfChanged(ref _totalRecords, value);
    // }

    public int TotalRecords => _pagedResults?.Count ?? 0;

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Održavanje audio zapisa";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "audio-manager";
    public IScreen HostScreen     { get; }

    #endregion

}