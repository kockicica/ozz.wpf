using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Views;
using ozz.wpf.Views.Dialogs;
using ozz.wpf.Views.Equalizer;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class DispositionViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {
    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly IClient                                                 _client;
    private readonly IEqualizerPresetFactory                                 _equalizerPresetFactory;
    private readonly ILogger<DialogWindowViewModel>                          _logger;
    private readonly IResolver                                               _resolver;
    private          ObservableAsPropertyHelper<IEnumerable<Category>>       _categories;
    private          ObservableAsPropertyHelper<IEnumerable<AudioRecording>> _recordings;
    private          bool?                                                   _searchActive = true;
    private          DateTimeOffset?                                         _searchFrom;
    private          Subject<bool>                                           _searchSubject = new();
    private          string                                                  _searchTerm;
    private          DateTimeOffset?                                         _searchTo;
    private          Category?                                               _selectedCategory;
    private          AudioRecording?                                         _selectedRecording;

    public DispositionViewModel(IClient client, ILogger<DialogWindowViewModel> logger, IEqualizerPresetFactory equalizerPresetFactory,
                                IResolver resolver, IAudioRecordingsService audioRecordingsService, IScreen screen) {

        _client = client;
        _logger = logger;
        _equalizerPresetFactory = equalizerPresetFactory;
        _resolver = resolver;
        _audioRecordingsService = audioRecordingsService;
        HostScreen = screen;

        ShowPlayer = new Interaction<AudioRecording, Unit>();

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
                                  .Select(_ => true),
                              this.WhenAnyValue(x => x.SearchTerm).Throttle(TimeSpan.FromMilliseconds(500)).Select(_ => true),
                              this.WhenAnyValue(x => x.SearchActive, x => x.SearchFrom, x => x.SearchTo).Select(_ => true)
                          )
                          .SelectMany(
                              Observable
                                  .FromAsync(ExecuteAsyncSearch)
                                  .Catch(Observable.Return(new PagedResults<AudioRecording>()))
                          )
                          .Select(x => x.Data)
                          .ToProperty(this, x => x.Recordings).DisposeWith(d);

            ShowPlayer.RegisterHandler(DoShowDialogAsync).DisposeWith(d);

            // select first item on any recordings list update
            this.WhenAnyValue(model => model.Recordings)
                .Where(recordings => recordings != null)
                .Subscribe(recordings => SelectedRecording = recordings.FirstOrDefault())
                .DisposeWith(d);
        });

    }


    public IEnumerable<Category> Categories {
        get => _categories?.Value ?? new Category[] { new() { Id = 1, Name = "test", Order = 1 } };
    }

    public IEnumerable<AudioRecording> Recordings {
        get => _recordings?.Value;
    }

    public Category? SelectedCategory {
        get => _selectedCategory;
        set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
    }

    [NotNull]
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

    [NotNull]
    public Interaction<AudioRecording, Unit> ShowPlayer { get; }

    public ReactiveCommand<string?, Unit> ClearDate { get; set; }

    public bool? SearchActive {
        get => _searchActive;
        set => this.RaiseAndSetIfChanged(ref _searchActive, value);
    }

    public DateTimeOffset? SearchFrom {
        get => _searchFrom;
        set => this.RaiseAndSetIfChanged(ref _searchFrom, value);
    }

    public DateTimeOffset? SearchTo {
        get => _searchTo;
        set => this.RaiseAndSetIfChanged(ref _searchTo, value);
    }

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
        await ShowPlayer.Handle(recording);
    }

    private Task<PagedResults<AudioRecording>> ExecuteAsyncSearch(CancellationToken token) {

        var sp = new AudioRecordingsSearchParams {
            CategoryId = SelectedCategory.Id,
            Name = SearchTerm,
            Active = SearchActive,
            FromDate = SearchFrom?.Date,
            ToDate = SearchTo?.Date,
        };
        return _audioRecordingsService.AudioRecordings(sp, token);
    }

    private async Task DoShowDialogAsync(InteractionContext<AudioRecording, Unit> interactionContext) {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            if (desktop.MainWindow is MainWindow wnd) {

                var vm = _resolver.GetService<ModalAudioPlayerViewModel>();
                var pl = _resolver.GetService<AudioPlayerViewModel>();

                pl.Track = interactionContext.Input;

                vm.PlayerModel = pl;
                vm.AutoPlay = true;
                vm.EqualizerViewModel = new EqualizerViewModel {
                    //Equalizer = (await _equalizerPresetFactory.GetPresets()).FirstOrDefault()
                };
                vm.Equalizers = new ObservableCollection<Equalizer>(await _equalizerPresetFactory.GetPresets());
                vm.EqualizerViewModel.Equalizer = await _equalizerPresetFactory.GetDefaultPreset();

                var modal = new ModalAudioPlayerWindow {
                    DataContext = vm
                };
                //wnd.ShowOverlay();
                await modal.ShowDialog(wnd);
                //await Task.Delay(1000);
                //wnd.HideOverlay();

            }

        }

        interactionContext.SetOutput(Unit.Default);

    }
}