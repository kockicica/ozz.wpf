using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Services.Interactions;

using ReactiveUI;

using File = TagLib.File;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.ViewModels;

public class CreateAudioRecordingViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel, ICaption {
    private readonly IBrowseForFile _browseForFile;
    private readonly IClient        _dataClient;

    private readonly ILogger<CreateAudioRecordingViewModel> _logger;
    private readonly INotificationManager                   _notificationManager;

    private bool                 _active;
    private AudioPlayerViewModel _audioPlayerViewModel;

    private ObservableAsPropertyHelper<IEnumerable<Category?>> _categories;
    private string                                             _client;
    private string                                             _comment;
    private TimeSpan                                           _duration;
    private string                                             _fileName;

    private string    _name;
    private Category? _selectedCategory;

    public CreateAudioRecordingViewModel() {
    }

    public CreateAudioRecordingViewModel(ILogger<CreateAudioRecordingViewModel> logger, IScreen hostScreen, IClient client,
                                         INotificationManager notificationManager, IBrowseForFile browseForFile,
                                         AudioPlayerViewModel audioPlayerViewModel) {

        _logger = logger;
        _dataClient = client;
        _notificationManager = notificationManager;
        _browseForFile = browseForFile;
        HostScreen = hostScreen;
        AudioPlayerViewModel = audioPlayerViewModel;

        BrowseForAudioFile = ReactiveCommand.CreateFromObservable<Unit, string?>(
            unit => _browseForFile.Browse.Handle(new BrowseForFileConfig { Title = "Pronađite audio fajl", Filters = MakeFileDialogFilters() }));

        var canCreateAudioFile = this.WhenAnyValue(x => x.SelectedCategory,
                                                   x => x.Name,
                                                   x => x.FileName,
                                                   (category, s, filePath)
                                                       => category != null && !string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(filePath));

        CreateAudioFile = ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(HandleCreateAudioFile, canCreateAudioFile);
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
                        Duration = Duration.Milliseconds * 1_000_000,
                        Name = Name,
                        Path = FileName,
                    };
                })
                .DisposeWith(d);

            CreateAudioFile
                .Catch(Observable.Return<AudioRecording?>(null))
                .Where(recording => recording != null)
                .Subscribe(recording => {
                    // created ok, now what?
                    _notificationManager.Show(new Notification("Obaveštenje", "Audio zapis je uspešno kreiran", NotificationType.Success));
                    HandleClear();
                })
                .DisposeWith(d);

            CreateAudioFile
                .ThrownExceptions
                .Where(exception => exception is AudioRecordingCreateException)
                .Subscribe(exception => {
                    _notificationManager.Show(new Notification("Greška",
                                                               $"Problem prilikom kreiranja audio zapisa:\n{exception.Message}",
                                                               NotificationType.Error));
                })
                .DisposeWith(d);

            _categories = _dataClient.Categories().ToObservable().ToProperty(this, x => x.Categories).DisposeWith(d);
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

    public ReactiveCommand<Unit, AudioRecording?> CreateAudioFile { get; set; }

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

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption => "Kreiranje novog zapisa";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "create-record";
    public IScreen HostScreen     { get; }

    #endregion

    private Task<AudioRecording?> HandleCreateAudioFile(Unit arg) {
        var af = new CreateAudioRecording {
            Active = Active,
            Comment = Comment,
            Date = DateTime.Now,
            Duration = Duration,
            Name = Name,
            Path = FileName,
            Category = SelectedCategory!.Name,
        };
        return _dataClient.Create(af);
    }

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