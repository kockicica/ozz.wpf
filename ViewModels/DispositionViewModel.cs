using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Views;

using ReactiveUI;

using Splat;

using EqualizerModel = ozz.wpf.Models.Equalizer;

namespace ozz.wpf.ViewModels;

public class DispositionViewModel : ViewModelBase, IActivatableViewModel {

    private readonly IClient                                            _client;
    private readonly IAudioRecordingsService                                 _audioRecordingsService;
    private readonly ILogger<DialogWindowViewModel>                          _logger;
    private readonly IEqualizerPresetFactory                                 _equalizerPresetFactory;
    private readonly IResolver                                               _resolver;
    private          ObservableAsPropertyHelper<IEnumerable<Category>>       _categories;
    private          ObservableAsPropertyHelper<IEnumerable<AudioRecording>> _recordings;
    private          string                                                  _searchTerm;
    private          Category                                                _selectedCategory;
    private          AudioRecording                                          _selectedRecording;

    public DispositionViewModel(IClient client, ILogger<DialogWindowViewModel> logger, IEqualizerPresetFactory equalizerPresetFactory,
                                IResolver resolver, IAudioRecordingsService audioRecordingsService) {

        _client = client;
        _logger = logger;
        _equalizerPresetFactory = equalizerPresetFactory;
        _resolver = resolver;
        _audioRecordingsService = audioRecordingsService;

        ShowPlayer = new Interaction<AudioRecording, Unit>();

        ProcessCategory = ReactiveCommand.Create<Category>(cat => SelectedCategory = cat, Observable.Return(true));

        async void Execute(AudioRecording recording) {
            await ShowPlayer.Handle(recording);
        }

        ViewPlayerCommand = ReactiveCommand.Create<AudioRecording>(Execute);

        this.WhenActivated(d => {

            _categories = client
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
                                    .SelectMany(_ => _audioRecordingsService.AudioRecordingsForCategory(SelectedCategory.Id, SearchTerm).ToObservable()
                                                                 .Catch(Observable.Return(new List<AudioRecording>())))
                                    .ToProperty(this, x => x.Recordings).DisposeWith(d);

            _logger.LogDebug("Test log");

            var interval = TimeSpan.FromSeconds(10);
            Observable
                .Timer(interval, interval)
                .Subscribe(x => {
                    /* do smth every 5m */
                })
                .DisposeWith(d);

            ShowPlayer.RegisterHandler(DoShowDialogAsync).DisposeWith(d);
        });

    }


    public IEnumerable<Category> Categories {
        get => _categories?.Value ?? new Category[]{new() {Id = 1, Name = "test", Order = 1}};
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

    [NotNull]
    public AudioRecording SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public ReactiveCommand<Category, Unit> ProcessCategory { get; }

    public ReactiveCommand<AudioRecording, Unit> ViewPlayerCommand { get; set; }

    [NotNull]
    public Interaction<AudioRecording, Unit> ShowPlayer { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion


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
                vm.Equalizers = new ObservableCollection<EqualizerModel>(await _equalizerPresetFactory.GetPresets());
                vm.EqualizerViewModel.Equalizer = await _equalizerPresetFactory.GetDefaultPreset();

                var modal = new ModalAudioPlayerWindow {
                    DataContext = vm
                };
                wnd.ShowOverlay();
                await modal.ShowDialog(wnd);
                //await Task.Delay(1000);
                wnd.HideOverlay();

            }

        }

        interactionContext.SetOutput(Unit.Default);

        //return Task.CompletedTask;
    }
}