using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;

using JetBrains.Annotations;

using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

using Serilog;

namespace ozz.wpf.ViewModels;

public class DispositionViewModel : ViewModelBase, IActivatableViewModel {

    private ObservableAsPropertyHelper<IEnumerable<Category>> _categories;

    private ObservableAsPropertyHelper<IEnumerable<AudioRecording>> _recordings;

    private readonly ILogger _logger;

    private Category _selectedCategory;

    private string _searchTerm;

    private readonly IDataService _dataService;

    public DispositionViewModel(IDataService dataService, ILogger logger) {

        _dataService = dataService;
        _logger = logger;

        ShowPlayer = new Interaction<AudioRecording, Unit>();

        ProcessCategory = ReactiveCommand.Create<Category>(cat => SelectedCategory = cat, Observable.Return(true));

        async void Execute(AudioRecording recording) {
            await ShowPlayer.Handle(recording);
            var c = 100;
        }

        ViewPlayerCommand = ReactiveCommand.Create<AudioRecording>(Execute);

        this.WhenActivated(d => {

            _categories = dataService
                          .Categories()
                          .ToObservable()
                          .Do(categories => { SelectedCategory = categories.FirstOrDefault()!; })
                          .Catch(Observable.Return(new List<Category> { new() { Id = 1, Name = "Errr", Order = 1 } }))
                          .ToProperty(this, x => x.Categories).DisposeWith(d);


            _recordings = Observable.Merge<object?>(
                                        this.WhenAnyValue(model => model.SelectedCategory),
                                        this.WhenAnyValue(model => model.SearchTerm).Throttle(TimeSpan.FromMilliseconds(300))
                                    )
                                    .Where(x => x != null)
                                    .SelectMany(_ => _dataService.AudioRecordingsForCategory(SelectedCategory.Id, SearchTerm).ToObservable()
                                                                 .Catch(Observable.Return(new List<AudioRecording>())))
                                    .ToProperty(this, x => x.Recordings).DisposeWith(d);

            _logger.Debug("Test log");

            var interval = TimeSpan.FromSeconds(10);
            Observable
                .Timer(interval, interval)
                .Subscribe(x => {
                    /* do smth every 5m */
                })
                .DisposeWith(d);
        });

    }


    public IEnumerable<Category> Categories {
        get => _categories?.Value;
    }

    public IEnumerable<AudioRecording> Recordings {
        get => _recordings?.Value;
    }

    [NotNull]
    public Category SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    [NotNull]
    public string SearchTerm {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    private AudioRecording _selectedRecording;

    [NotNull]
    public AudioRecording SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public ReactiveCommand<Category, Unit> ProcessCategory { get; }

    public ViewModelActivator Activator { get; } = new();

    public ReactiveCommand<AudioRecording, Unit> ViewPlayerCommand { get; set; }

    [NotNull]
    public Interaction<AudioRecording, Unit> ShowPlayer { get; }

}