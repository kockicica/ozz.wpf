using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {
    private readonly IAudioRecordingsService _audioRecordingsService;
    private readonly IClient                 _client;

    private readonly ILogger<AudioRecordingsManagerViewModel> _logger;

    private ObservableAsPropertyHelper<IEnumerable<Category?>> _categories;
    private IEnumerable<AudioRecording>                        _results;

    private AudioRecordingsSearchParams _searchParams = new();

    private Category? _selectedCategory;

    public AudioRecordingsManagerViewModel(ILogger<AudioRecordingsManagerViewModel> logger, IScreen screen, IClient client,
                                           IAudioRecordingsService audioRecordingsService) {

        _logger = logger;
        HostScreen = screen;
        _client = client;
        _audioRecordingsService = audioRecordingsService;

        Search = ReactiveCommand.CreateFromTask<AudioRecordingsSearchParams, PagedResults<AudioRecording>>(
            sp => _audioRecordingsService.AudioRecordings(sp));


        this.WhenActivated(d => {
            _categories = _client
                          .Categories().ToObservable()
                          .AddNull()
                          .ToProperty(this, x => x.Categories)
                          .DisposeWith(d);

            this.WhenAnyValue(model => model.SelectedCategory).Subscribe(category => { SearchParams.CategoryId = category?.Id; }).DisposeWith(d);

            Search.Subscribe(results => Results = results.Data).DisposeWith(d);
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

    public IEnumerable<AudioRecording> Results {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public ReactiveCommand<AudioRecordingsSearchParams, PagedResults<AudioRecording>> Search { get; set; }

    public Category? SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

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