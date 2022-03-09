using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;
using DynamicData.Binding;

using LibVLCSharp.Shared;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Controls;
using ozz.wpf.Dialog;
using ozz.wpf.Services;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public class DispositionBlockViewModel : DialogViewModelBase {
    private readonly IAppStateManager        _appStateManager;
    private readonly IEqualizerPresetFactory _equalizerPreset;

    private readonly ILogger<DispositionBlockViewModel> _logger;
    private          AudioPlayerConfiguration           _audioPlayerConfiguration;

    private AudioPlayerViewModel _audioPlayerViewModel;

    private MediaList? _blockMedia;

    private Models.Disposition? _currentDisposition;

    private string? _currentTrackName;

    private DispositionBlock? _dispositionBlock;
    // private ObservableCollection<Models.Disposition> _dispositionsToEmit = new();
    // private ObservableCollection<Models.Disposition> _emittedDispositions = new();

    private Models.Equalizer                 _equalizer = Models.Equalizer.Default;
    private bool                             _isPaused  = true;
    private bool                             _isStopped = true;
    private LibVLC                           _libVLC;
    private long                             _mediaLength = 1;
    private MediaPlayer                      _mediaPlayer;
    private ObservableAsPropertyHelper<long> _mediaPosition;

    private ObservableAsPropertyHelper<MediaPlayerState> _playerState;

    private ServerConfiguration                   _serverConfiguration;
    private ObservableAsPropertyHelper<TimeSpan>? _totalDispositionsToEmitDuration;

    private ObservableAsPropertyHelper<TimeSpan>? _totalEmittedDuration;
    //private int                                   _volume = 30;


    public DispositionBlockViewModel(ILogger<DispositionBlockViewModel> logger, IOptions<ServerConfiguration> serverConfigurationOptions,
                                     IOptions<AudioPlayerConfiguration> audioPlayerConfigurationOptions, IEqualizerPresetFactory equalizerPreset,
                                     IAppStateManager appStateManager) {
        _logger = logger;
        _equalizerPreset = equalizerPreset;
        _appStateManager = appStateManager;
        _libVLC = new LibVLC("--no-video");
        _mediaPlayer = new MediaPlayer(_libVLC);
        _serverConfiguration = serverConfigurationOptions.Value;
        //_volume = audioPlayerConfigurationOptions.Value.Volume.GetValueOrDefault();
        Player.Volume = _appStateManager.State.Volume;

        this.WhenActivated(d => {

            Observable
                .FromAsync(token => _equalizerPreset.GetDefaultPreset())
                .Subscribe(equalizer => { Equalizer = equalizer; })
                .DisposeWith(d);

            _playerState
                = Observable.Merge(
                                Observable
                                    .FromEventPattern(_mediaPlayer, "Playing")
                                    .Select(_ => MediaPlayerState.Playing),
                                Observable
                                    .FromEventPattern(_mediaPlayer, "Paused")
                                    .Select(_ => MediaPlayerState.Paused),
                                Observable
                                    .FromEventPattern(_mediaPlayer, "Stopped")
                                    .Select(_ => MediaPlayerState.Stopped)
                                //.Do(_ => { Player.Volume = Volume; })
                            )
                            .ToProperty(this, x => x.PlayerState)
                            .DisposeWith(d);

            _mediaPosition
                = Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, nameof(MediaPlayer.TimeChanged))
                            .Select(x => { return EmittedDispositions.Sum(d => d.Duration / 1_000_000) + x.EventArgs.Time; })
                            .ToProperty(this, x => x.MediaPosition)
                            .DisposeWith(d);


            _totalDispositionsToEmitDuration
                = Observable.Merge(
                                DispositionsToEmit
                                    .ToObservableChangeSet()
                                    .Select(_ => (long)0),
                                Observable
                                    .FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, nameof(MediaPlayer.TimeChanged))
                                    .Select(x => -1 * x.EventArgs.Time)
                            )
                            .Select(dd => DispositionsToEmit.Sum(dsp => dsp.Duration / 1_000_000) + dd)
                            .Select(dd => CurrentDisposition != null ? dd + CurrentDisposition.Duration / 1_000_000 : dd)
                            .Select(x => TimeSpan.FromMilliseconds(x))
                            .ToProperty(this, x => x.TotalDispositionsToEmitDuration)
                            .DisposeWith(d);

            _totalEmittedDuration
                = Observable.Merge(
                                EmittedDispositions
                                    .ToObservableChangeSet()
                                    .Select(_ => (long)0),
                                Observable
                                    .FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, nameof(MediaPlayer.TimeChanged))
                                    .Select(x => x.EventArgs.Time)
                            )
                            .Select(dd => EmittedDispositions.Sum(dsp => dsp.Duration / 1_000_000) + dd)
                            .Select(x => TimeSpan.FromMilliseconds(x))
                            .ToProperty(this, x => x.TotalEmittedDuration)
                            .DisposeWith(d);



            Observable.FromEventPattern<EventArgs>(_mediaPlayer, nameof(MediaPlayer.EndReached))
                      .ObserveOn(RxApp.MainThreadScheduler)
                      .Subscribe(HandleMediaPlayerEndReached)
                      .DisposeWith(d);

            Observable.FromEventPattern<MediaPlayerMediaChangedEventArgs>(_mediaPlayer, nameof(MediaPlayer.MediaChanged))
                      .Subscribe(pattern => {
                          var a = pattern;
                      })
                      .DisposeWith(d);

            Disposable.Create(() => {
                          if (Player.IsPlaying) Player.Stop();
                      })
                      .DisposeWith(d);

            _libVLC.DisposeWith(d);
        });
    }

    public DispositionBlock? Block {
        get => _dispositionBlock;
        set {
            _dispositionBlock = value;

            EmittedDispositions.Clear();
            if (_dispositionBlock?.Dispositions != null) {
                DispositionsToEmit.AddRange(_dispositionBlock.Dispositions);
            }
            CurrentDisposition = Next(DispositionsToEmit);

            MediaLength = (long)_dispositionBlock.TotalDuration.TotalMilliseconds;

            this.RaisePropertyChanged(nameof(Block));
            this.RaisePropertyChanged(nameof(DispositionsToEmit));
        }
    }

    public MediaPlayer Player => _mediaPlayer;

    public int Volume {
        get => _appStateManager.State.Volume;
        set {
            _appStateManager.State.Volume = value;
            Player.Volume = value;
        }
    }

    public Models.Equalizer Equalizer {
        get => _equalizer;
        set {
            Player.SetEqualizer(FromModel(value));
            this.RaiseAndSetIfChanged(ref _equalizer, value);
        }
    }

    public MediaPlayerState PlayerState => _playerState?.Value ?? MediaPlayerState.Stopped;

    public long MediaLength {
        get => _mediaLength;
        set => this.RaiseAndSetIfChanged(ref _mediaLength, value);
    }

    public long MediaPosition => _mediaPosition?.Value ?? 0;

    public Models.Disposition? CurrentDisposition {
        get => _currentDisposition;
        set {
            this.RaiseAndSetIfChanged(ref _currentDisposition, value);
            if (_currentDisposition != null) {
                CurrentTrackName = $"{_currentDisposition.Order}. - {_currentDisposition.Name}";
            }
        }
    }

    public ObservableCollection<Models.Disposition> EmittedDispositions { get; set; } = new();

    public ObservableCollection<Models.Disposition> DispositionsToEmit { get; set; } = new();

    public TimeSpan TotalEmittedDuration => _totalEmittedDuration?.Value ?? TimeSpan.Zero;

    public TimeSpan TotalDispositionsToEmitDuration => _totalDispositionsToEmitDuration?.Value ?? TimeSpan.Zero;

    public string? CurrentTrackName {
        get => _currentTrackName;
        set => this.RaiseAndSetIfChanged(ref _currentTrackName, value);
    }

    public void HandlePlay() {
        if (CurrentDisposition != null) {
            Player.Media = FromDisposition(CurrentDisposition!);
        }
        Player.Stop();
        Player.Volume = Volume;
        Player.Play();
        Player.Volume = Volume;
    }

    public void HandlePause() {
        Player.Pause();
    }

    // public void HandleStop() {
    //     Player.Stop();
    //     Player.Volume = Volume;
    // }

    public void HandleSeek(AudioPlayerSeekEventArgs eventArgs) {
        var totalPosition = TimeSpan.FromMilliseconds((double)eventArgs.Position) -
                            TimeSpan.FromMilliseconds(EmittedDispositions.Sum(d => d.Duration / 1_000_000));
        if (totalPosition.TotalMilliseconds < 0) {
            totalPosition = TimeSpan.Zero;
        }
        Player.SeekTo(totalPosition);
    }

    public void HandlePlayOrPause() {
        if (PlayerState == MediaPlayerState.Stopped) {
            HandlePlay();
        }
        else {
            HandlePause();
        }
    }

    private void HandleMediaPlayerEndReached(EventPattern<EventArgs> d) {

        CurrentDisposition!.IncreasePlayCount();

        DispositionsToEmit.Remove(CurrentDisposition!);
        EmittedDispositions.Add(CurrentDisposition!);
        CurrentDisposition = Next(DispositionsToEmit);
        if (CurrentDisposition != null) {
            Player.Play(FromDisposition(CurrentDisposition));
        }
        Player.Volume = Volume;
    }

    private LibVLCSharp.Shared.Equalizer FromModel(Models.Equalizer mdl) {
        var eq = new LibVLCSharp.Shared.Equalizer();
        eq.SetPreamp((float)mdl.PreAmp);
        foreach (var equalizerBand in mdl.Bands) {
            eq.SetAmp((float)equalizerBand.Amp, (uint)equalizerBand.Number);
        }
        return eq;

    }

    private Media FromDisposition(Models.Disposition disposition) {
        var url = new Uri($"{_serverConfiguration.Url}/api/audio/media/{disposition.Id}");
        var media = new Media(_libVLC, url);
        return media;
    }

    private Models.Disposition? Next(IList<Models.Disposition> items) {
        Models.Disposition? rec = null;
        switch (items.Count) {
            case > 0:
                rec = items[0];
                items.RemoveAt(0);
                break;
        }
        return rec;
    }
}