using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Services;
using ozz.wpf.Views.Equalizer;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.Views.Dialogs;

public class ModalAudioPlayerViewModel : DialogViewModelBase<DialogResultBase> {

    private readonly IEqualizerPresetFactory _equalizerPresetFactory;

    private bool _equalizerOn = true;

    private ObservableCollection<Models.Equalizer> _equalizers;

    private ILogger<ModalAudioPlayerViewModel> _logger;

    public ModalAudioPlayerViewModel(ILogger<ModalAudioPlayerViewModel> logger, IEqualizerPresetFactory equalizerPresetFactory) {

        _logger = logger;
        _equalizerPresetFactory = equalizerPresetFactory;

        ToggleEqualizer = ReactiveCommand.Create(() => { EqualizerOn = !EqualizerOn; });

        this.WhenActivated(d => {
            this.WhenAnyValue(x => x.EqualizerOn)
                .Subscribe(b => { PlayerModel!.Equalizer = b ? EqualizerViewModel!.Equalizer : Models.Equalizer.Default; })
                .DisposeWith(d);

            EqualizerViewModel!
                .EqualizerUpdated
                .Where(_ => EqualizerOn)
                .Subscribe(equalizer => { PlayerModel!.Equalizer = equalizer; })
                .DisposeWith(d);

            EqualizerViewModel!
                .EqualizerUpdated
                .Throttle(TimeSpan.FromMilliseconds(500))
                .SelectMany(equalizer => _equalizerPresetFactory.SavePreset(equalizer).ToObservable())
                .Subscribe(equalizer => {
                    //PlayerModel!.Equalizer = equalizer;
                    EqualizerViewModel.Equalizer.Id = equalizer.Id;
                })
                .DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> ToggleEqualizer { get; set; }

    public AudioPlayerViewModel PlayerModel { get; set; }

    public EqualizerViewModel EqualizerViewModel { get; set; }

    public ObservableCollection<Models.Equalizer> Equalizers {
        get => _equalizers;
        set => this.RaiseAndSetIfChanged(ref _equalizers, value);
    }

    public bool EqualizerOn {
        get => _equalizerOn;
        set => this.RaiseAndSetIfChanged(ref _equalizerOn, value);
    }

    public bool AutoPlay { get; set; }
}