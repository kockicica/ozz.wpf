using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {

    private readonly IAudioRecordingsService _audioRecordingsService;
    private readonly IBrowseForFile          _browseForFile;
    private readonly IClient                 _client;

    private readonly ILogger<AudioRecordingsManagerViewModel> _logger;

    private ObservableAsPropertyHelper<IEnumerable<Category?>> _categories;

    private bool                                 _isUpdate;
    private ObservableCollection<AudioRecording> _results;

    private AudioRecordingsSearchParams _searchParams = new();

    private Category? _selectedCategory;

    private AudioRecording? _selectedRecording;


    public AudioRecordingsManagerViewModel(ILogger<AudioRecordingsManagerViewModel> logger, IScreen screen, IClient client,
                                           IAudioRecordingsService audioRecordingsService, IBrowseForFile browseForFile) {

        _logger = logger;
        HostScreen = screen;
        _client = client;
        _audioRecordingsService = audioRecordingsService;
        _browseForFile = browseForFile;

        Search = ReactiveCommand.CreateFromTask<AudioRecordingsSearchParams, PagedResults<AudioRecording>>(
            sp => _audioRecordingsService.AudioRecordings(sp));

        async Task<AudioRecording?> HandleEditRecording(AudioRecording recording) {
            var res = await _browseForFile.EditAudioRecording.Handle(recording);
            return res;
        }

        EditRecording = ReactiveCommand.CreateFromTask<AudioRecording, AudioRecording?>(HandleEditRecording);


        this.WhenActivated(d => {
            _categories = _client
                          .Categories().ToObservable()
                          .AddNull()
                          .ToProperty(this, x => x.Categories)
                          .DisposeWith(d);

            this.WhenAnyValue(model => model.SelectedCategory).Subscribe(category => { SearchParams.CategoryId = category?.Id; }).DisposeWith(d);

            Search.Subscribe(results => Results = new ObservableCollection<AudioRecording>(results.Data)).DisposeWith(d);

            EditRecording
                .Where(recording => recording != null)
                .Subscribe(recording => {
                    var rec = Results.SingleOrDefault(x => x.Id == recording!.Id);
                    if (rec != null) {
                        rec.Name = recording!.Name;
                        //Results = new ObservableCollection<AudioRecording>(Results);
                        this.RaisePropertyChanged(nameof(Results));
                    }
                })
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

    public ObservableCollection<AudioRecording> Results {
        get => _results;
        set => this.RaiseAndSetIfChanged(ref _results, value);
    }

    public ReactiveCommand<AudioRecordingsSearchParams, PagedResults<AudioRecording>> Search { get; set; }

    public ReactiveCommand<AudioRecording, AudioRecording?> EditRecording { get; set; }

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