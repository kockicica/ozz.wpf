using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;

using Avalonia.Controls;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class CreateAudioRecordingViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel, ICaption {

    private readonly ILogger<CreateAudioRecordingViewModel> _logger;
    private readonly IMainWindowProvider                    _mainWindowProvider;

    private bool      _active;
    private Category? _category;
    private string    _client;
    private string    _comment;
    private string    _duration;
    private string    _fileName;

    private string _name;

    public CreateAudioRecordingViewModel() {
    }

    public CreateAudioRecordingViewModel(ILogger<CreateAudioRecordingViewModel> logger, IScreen hostScreen, IMainWindowProvider mainWindowProvider) {

        _logger = logger;
        HostScreen = hostScreen;
        _mainWindowProvider = mainWindowProvider;

        BrowseForAudioFile = ReactiveCommand.CreateFromObservable<Unit, string?>(unit => OpenAudioFile.Handle(unit));

        this.WhenActivated(d => {
            OpenAudioFile
                .RegisterHandler(async context => {
                    var dlg = new OpenFileDialog { Title = "Pronađite audio fajl", Filters = MakeFileDialogFilters() };
                    var res = await dlg.ShowAsync(_mainWindowProvider.GetMainWindow()!);
                    context.SetOutput(res?[0]);
                })
                .DisposeWith(d);
            BrowseForAudioFile
                //.Catch(Observable.Return((string?)null))
                .Subscribe(fileName => { FileName = fileName; })
                .DisposeWith(d);
        });
    }

    public string Name {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public Category? Category {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public string Duration {
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

    public Interaction<Unit, string> OpenAudioFile { get; set; } = new();

    public ReactiveCommand<Unit, string?> BrowseForAudioFile { get; set; }

    public string FileName {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
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

    private List<FileDialogFilter> MakeFileDialogFilters() {
        return new List<FileDialogFilter> {
            new() { Name = "Audio datoteke", Extensions = new List<string> { "wav", "mp3", "flac" } }
        };
    }
}