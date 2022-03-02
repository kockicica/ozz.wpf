using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using LibVLCSharp.Shared;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public class DispositionBlockViewModel : DialogViewModelBase {

    private readonly ILogger<DispositionBlockViewModel> _logger;
    private          AudioPlayerConfiguration           _audioPlayerConfiguration;

    private AudioPlayerViewModel                 _audioPlayerViewModel;
    private MediaList?                           _blockMedia;
    private ObservableAsPropertyHelper<TimeSpan> _currentPlayerTime;
    private AudioRecording?                      _currentTrack;

    private DispositionBlock?                _dispositionBlock;
    private Models.Equalizer                 _equalizer = Models.Equalizer.Default;
    private bool                             _isPaused  = true;
    private bool                             _isStopped = true;
    private LibVLC                           _libVLC;
    private long                             _mediaLength = 1;
    private MediaPlayer                      _mediaPlayer;
    private ObservableAsPropertyHelper<long> _mediaPosition;

    private ObservableAsPropertyHelper<MediaPlayerState> _playerState;

    private ServerConfiguration _serverConfiguration;
    private int                 _volume = 30;

    public DispositionBlockViewModel(ILogger<DispositionBlockViewModel> logger, IOptions<ServerConfiguration> serverConfigurationOptions,
                                     IOptions<AudioPlayerConfiguration> audioPlayerConfigurationOptions, AudioPlayerViewModel audioPlayerViewModel) {
        _logger = logger;
        _audioPlayerViewModel = audioPlayerViewModel;
        _libVLC = new LibVLC("--no-video");
        _mediaPlayer = new MediaPlayer(_libVLC);
        _serverConfiguration = serverConfigurationOptions.Value;
        _volume = audioPlayerConfigurationOptions.Value.Volume.GetValueOrDefault();

        this.WhenActivated(d => {


            _playerState = Observable.Merge(
                                         Observable.FromEventPattern(_mediaPlayer, "Playing")
                                                   .Select(_ => MediaPlayerState.Playing)
                                                   .Do(state => {
                                                       IsPaused = false;
                                                       IsStopped = false;
                                                       MediaLength = _mediaPlayer.Length;
                                                   }),
                                         Observable.FromEventPattern(_mediaPlayer, "Paused")
                                                   .Select(_ => MediaPlayerState.Paused)
                                                   .Do(state => {
                                                       IsPaused = true;
                                                       IsStopped = false;
                                                   }),
                                         Observable.FromEventPattern(_mediaPlayer, "Stopped")
                                                   .Select(_ => MediaPlayerState.Stopped)
                                                   .Do(state => {
                                                       MediaLength = 1;
                                                       IsPaused = true;
                                                       IsStopped = true;
                                                       Player.Volume = Volume;
                                                   })
                                     )
                                     .ToProperty(this, x => x.PlayerState)
                                     .DisposeWith(d);
            _currentPlayerTime = Observable.Merge(
                                               Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, "TimeChanged")
                                                         .Select(x => TimeSpan.FromMilliseconds(x.EventArgs.Time)),
                                               Observable.FromEventPattern(_mediaPlayer, "Stopped").Select(_ => MediaPlayerState.Stopped)
                                                         .Select(_ => TimeSpan.Zero)
                                           )
                                           .ToProperty(this, x => x.CurrentPlayerTime)
                                           .DisposeWith(d);

            _mediaPosition = Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, "TimeChanged")
                                       .Select(x => x.EventArgs.Time)
                                       .ToProperty(this, x => x.MediaPosition)
                                       .DisposeWith(d);

            Observable.FromEventPattern(_mediaPlayer, "EndReached")
                      .Subscribe(_ => Player.Volume = Volume)
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
            _blockMedia = new MediaList(_libVLC);
            foreach (var disposition in _dispositionBlock.Dispositions) {
                _blockMedia.AddMedia(FromDisposition(disposition));
            }
            this.RaisePropertyChanged(nameof(Block));
        }
    }

    public MediaPlayer Player {
        get => _mediaPlayer;
    }

    public int Volume {
        get => _volume;
        set {
            Player.Volume = value;
            this.RaiseAndSetIfChanged(ref _volume, value);

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

    public TimeSpan CurrentPlayerTime => _currentPlayerTime?.Value ?? TimeSpan.Zero;

    public bool IsPaused {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    public bool IsStopped {
        get => _isStopped;
        set => this.RaiseAndSetIfChanged(ref _isStopped, value);
    }

    public long MediaLength {
        get => _mediaLength;
        set => this.RaiseAndSetIfChanged(ref _mediaLength, value);
    }

    public long MediaPosition => _mediaPosition?.Value ?? 0;

    public AudioRecording? CurrentTrack {
        get => _currentTrack;
        set {
            AudioPlayerViewModel.Track = value;
            this.RaiseAndSetIfChanged(ref _currentTrack, value);
        }
    }

    public AudioPlayerViewModel AudioPlayerViewModel {
        get => _audioPlayerViewModel;
        set => this.RaiseAndSetIfChanged(ref _audioPlayerViewModel, value);
    }

    public void HandlePlay() {

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
}