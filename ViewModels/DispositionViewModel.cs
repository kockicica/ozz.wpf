using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class DispositionViewModel : ViewModelBase, IActivatableViewModel {

    private readonly IDataService _dataService;

    private ObservableAsPropertyHelper<IEnumerable<Category>>       _categories;
    private ObservableAsPropertyHelper<IEnumerable<AudioRecording>> _recordings;

    private readonly ILogger _logger;

    private Category _selectedCategory;
    private string   _searchTerm;

    public DispositionViewModel(IDataService dataService, ILogger logger) {

        _dataService = dataService;
        _logger = logger;

        ProcessCategory = ReactiveCommand.Create<Category>(cat => SelectedCategory = cat, Observable.Return(true));

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

            _logger.LogDebug("Test log");

            // var interval = TimeSpan.FromMinutes(5);
            //
            // Observable
            //     .Timer(interval, interval)
            //     .Subscribe(x => { /* do smth every 5m */ })
            //     .DisposeWith(d);            
        });

        // _categories = dataService
        //               .Categories()
        //               .ToObservable()
        //               .Do(categories => { SelectedCategory = categories.FirstOrDefault()!; })
        //               .Catch(Observable.Return(new List<Category> { new() { Id = 1, Name = "Errr", Order = 1 } }))
        //               .ToProperty(this, x => x.Categories);
        //
        //
        // _recordings = Observable.Merge<object?>(
        //                             this.WhenAnyValue(model => model.SelectedCategory),
        //                             this.WhenAnyValue(model => model.SearchTerm).Throttle(TimeSpan.FromMilliseconds(300))
        //                         )
        //                         .Where(x => x != null)
        //                         .SelectMany(_ => _dataService.AudioRecordingsForCategory(SelectedCategory.Id, SearchTerm).ToObservable())
        //                         .ToProperty(this, x => x.Recordings);
        //
        // _logger.LogDebug("Test log");


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

    public ReactiveCommand<Category, Unit> ProcessCategory { get; }

    public ViewModelActivator Activator { get; } = new();

}