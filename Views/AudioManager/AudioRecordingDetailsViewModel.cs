using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;
using ozz.wpf.ViewModels;
using ozz.wpf.Views.Player;

using ReactiveUI;

using File = TagLib.File;

namespace ozz.wpf.Views.AudioManager;

public class AudioRecordingDetailsViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel, ICaption {
    private readonly IClient _dataClient;

    private readonly ILogger<AudioRecordingDetailsViewModel> _logger;
    private readonly INotificationManager                    _notificationManager;
    private readonly IOzzInteractions                        _ozzInteractions;

    private bool                 _active;
    private AudioPlayerViewModel _audioPlayerViewModel;

    private ObservableAsPropertyHelper<IEnumerable<Category?>> _categories;
    private string                                             _categoryName;
    private string                                             _client;
    private string                                             _comment;
    private TimeSpan                                           _duration;
    private string                                             _fileName;

    private bool _isUpdate;

    private string    _name;
    private Category? _selectedCategory;

    public AudioRecordingDetailsViewModel() {
    }

    public AudioRecordingDetailsViewModel(ILogger<AudioRecordingDetailsViewModel> logger, IScreen hostScreen, IClient client,
                                          INotificationManager notificationManager, IOzzInteractions ozzInteractions,
                                          AudioPlayerViewModel audioPlayerViewModel) {

        _logger = logger;
        _dataClient = client;
        _notificationManager = notificationManager;
        _ozzInteractions = ozzInteractions;
        HostScreen = hostScreen;
        AudioPlayerViewModel = audioPlayerViewModel;

        BrowseForAudioFile = ReactiveCommand.CreateFromObservable<Unit, string?>(
            unit => _ozzInteractions.Browse.Handle(new BrowseForFileConfig { Title = "Pronađite audio fajl", Filters = MakeFileDialogFilters() }));

        Valid = this.WhenAnyValue(x => x.SelectedCategory,
                                  x => x.Name,
                                  x => x.FileName,
                                  x => x.IsUpdate,
                                  (category, name, filename, update) => update
                                      ? !string.IsNullOrEmpty(name) && category != null
                                      : !string.IsNullOrEmpty(name) && category != null && !string.IsNullOrEmpty(filename));

        ClearFields = ReactiveCommand.Create(HandleClear);

        this.WhenActivated(d => {

            BrowseForAudioFile
                .Where(s => !string.IsNullOrEmpty(s))
                .Subscribe(fileName => {
                    FileName = fileName;
                    var tfile = File.Create(fileName);
                    Duration = tfile.Properties.Duration;
                    Name = Path.GetFileNameWithoutExtension(fileName);
                    Comment = tfile.Properties.Description;

                    AudioPlayerViewModel.Track = new AudioRecording {
                        Category = SelectedCategory?.Name,
                        Duration = (long)Duration.TotalMilliseconds * 1_000_000,
                        Name = Name,
                        Path = FileName,
                    };
                })
                .DisposeWith(d);

            _categories = _dataClient.Categories().ToObservable().Do(categories => {
                var selected = categories.SingleOrDefault(x => x.Name == CategoryName);
                SelectedCategory = selected;
            }).ToProperty(this, x => x.Categories).DisposeWith(d);
        });
    }

    public string Name {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public TimeSpan Duration {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    public bool Active {
        get => _active;
        set => this.RaiseAndSetIfChanged(ref _active, value);
    }

    public string Client {
        get => _client;
        set => this.RaiseAndSetIfChanged(ref _client, value);
    }

    public string Comment {
        get => _comment;
        set => this.RaiseAndSetIfChanged(ref _comment, value);
    }

    public ReactiveCommand<Unit, string?> BrowseForAudioFile { get; set; }

    public ReactiveCommand<Unit, Unit> ClearFields { get; set; }

    public string FileName {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }

    public IEnumerable<Category?> Categories => _categories?.Value;

    public Category? SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    public AudioPlayerViewModel AudioPlayerViewModel {
        get => _audioPlayerViewModel;
        set => this.RaiseAndSetIfChanged(ref _audioPlayerViewModel, value);
    }

    public bool IsUpdate {
        get => _isUpdate;
        set => this.RaiseAndSetIfChanged(ref _isUpdate, value);
    }

    public string CategoryName {
        get => _categoryName;
        set => this.RaiseAndSetIfChanged(ref _categoryName, value);
    }

    public IObservable<bool> Valid { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption => "Kreiranje novog zapisa";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "create-record";

    public IScreen HostScreen { get; }

    #endregion

    // private Task<AudioRecording?> HandleCreateAudioFile(Unit arg) {
    //     var af = new CreateAudioRecording {
    //         Active = Active,
    //         Comment = Comment,
    //         Date = DateTime.Now,
    //         Duration = Duration,
    //         Name = Name,
    //         Path = FileName,
    //         Category = SelectedCategory!.Name,
    //     };
    //     return _dataClient.Create(af);
    // }

    private List<FileDialogFilter> MakeFileDialogFilters() {
        return new List<FileDialogFilter> {
            new() { Name = "Audio datoteke", Extensions = new List<string> { "wav", "mp3", "flac" } }
        };
    }

    private void HandleClear() {
        Name = "";
        Duration = TimeSpan.Zero;
        FileName = "";
        Comment = "";
        Client = "";
        SelectedCategory = null;
        AudioPlayerViewModel.Track = null;
    }
}