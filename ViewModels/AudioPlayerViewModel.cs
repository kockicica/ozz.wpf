using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

using LibVLCSharp.Shared;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;

using ReactiveUI;

using Equalizer = LibVLCSharp.Shared.Equalizer;
using EqualizerModel = ozz.wpf.Models.Equalizer;

namespace ozz.wpf.ViewModels;

public class AudioPlayerViewModel : DialogViewModelBase<DialogResultBase> {

    private readonly ILogger<AudioPlayerViewModel> _logger;

    private ObservableAsPropertyHelper<TimeSpan> _currentPlayerTime;

    private bool _isPaused = true;

    private bool _isStopped = true;

    private LibVLC _libVLC;

    private Media _media;

    private long _mediaLength = 1;

    private MediaPlayer _mediaPlayer;

    private ObservableAsPropertyHelper<long> _mediaPosition;

    private ObservableAsPropertyHelper<MediaPlayerState> _playerState;

    private AudioRecording? _track;

    private int _volume = 30;

    private EqualizerModel _equalizer = EqualizerModel.Default;

    public AudioPlayerViewModel(ILogger<AudioPlayerViewModel> logger) {

        _logger = logger;
        _libVLC = new LibVLC("--no-video");
        _mediaPlayer = new MediaPlayer(_libVLC);

        Play = ReactiveCommand.Create(HandlePlay);
        Pause = ReactiveCommand.Create(HandlePause);
        Stop = ReactiveCommand.Create(HandleStop);
        PlayOrPause = ReactiveCommand.Create(HandlePlayOrPause);

        // var eq = new Equalizer();
        // var count = eq.PresetCount;
        // var bc = eq.BandCount;
        // for (uint i = 0; i < count; i++) {
        //     var pName = eq.PresetName(i);
        //     _logger.Debug("Preseet name: {@name}", pName);
        // }


        this.WhenActivated(disposables => {

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
                                     .DisposeWith(disposables);

            _currentPlayerTime = Observable.Merge(
                                               Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, "TimeChanged")
                                                         .Select(x => TimeSpan.FromMilliseconds(x.EventArgs.Time)),
                                               Observable.FromEventPattern(_mediaPlayer, "Stopped").Select(_ => MediaPlayerState.Stopped)
                                                         .Select(_ => TimeSpan.Zero)
                                           )
                                           .ToProperty(this, x => x.CurrentPlayerTime)
                                           .DisposeWith(disposables);

            _mediaPosition = Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, "TimeChanged")
                                       .Select(x => x.EventArgs.Time)
                                       .ToProperty(this, x => x.MediaPosition)
                                       .DisposeWith(disposables);

            Observable.FromEventPattern(_mediaPlayer, "EndReached")
                      .Subscribe(_ => Player.Volume = Volume)
                      .DisposeWith(disposables);

            Disposable.Create(() => {
                if (Player.IsPlaying) Player.Stop();
            }).DisposeWith(disposables);


            _libVLC.DisposeWith(disposables);

        });

    }

    public ICommand Play { get; }

    public ICommand Pause { get; }

    public ICommand Stop { get; }

    public ICommand PlayOrPause { get; }

    public long MediaPosition => _mediaPosition?.Value ?? 0;

    public MediaPlayer Player => _mediaPlayer;

    public bool IsPaused {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }

    public bool IsStopped {
        get => _isStopped;
        set => this.RaiseAndSetIfChanged(ref _isStopped, value);
    }

    public MediaPlayerState PlayerState => _playerState?.Value ?? MediaPlayerState.Stopped;

    public TimeSpan CurrentPlayerTime => _currentPlayerTime?.Value ?? TimeSpan.Zero;

    public long MediaLength {
        get => _mediaLength;
        set => this.RaiseAndSetIfChanged(ref _mediaLength, value);
    }

    public AudioRecording? Track {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
    }

    public int Volume {
        get => _volume;
        set {
            this.RaiseAndSetIfChanged(ref _volume, value);
            Player.Volume = value;
        }
    }

    public EqualizerModel Equalizer {
        get => _equalizer;
        set {
            Player.SetEqualizer(FromModel(value));
            this.RaiseAndSetIfChanged(ref _equalizer, value);
        }
    }

    public void Seek(double position) {
        Player.SeekTo(TimeSpan.FromMilliseconds(position));
    }

    private void HandlePlayOrPause() {
        switch (PlayerState) {
            case MediaPlayerState.Stopped:
                HandlePlay();
                break;
            case MediaPlayerState.Paused:
                HandlePause();
                break;
            case MediaPlayerState.Playing:
                HandlePause();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandlePlay() {
        Player.Volume = Volume;
        Player.SetEqualizer(FromModel(Equalizer));

        //var eq = new Equalizer();
        //Player.SetEqualizer(new Equalizer());
        Player.Play(new Media(_libVLC, new Uri($"http://localhost:27000/api/audio/media/{Track.Id}")));

        //Player.Play(new Media(_libVLC, new Uri($"file://D:\\project\\source\\repositories\\ozzz\\ozz-ms\\artifacts\\media\\{Track.Path}")));
        //Player.Play(new Media(_libVLC, $"D:\\project\\source\\repositories\\ozzz\\ozz-ms\\artifacts\\{Track.Path}"));
    }

    private void HandlePause() {
        Player.Pause();
    }

    private void HandleStop() {
        Player.Stop();
    }

    private Equalizer FromModel(EqualizerModel mdl) {
        var eq = new Equalizer();
        eq.SetPreamp((float)mdl.PreAmp);
        foreach (var equalizerBand in mdl.Bands) {
            eq.SetAmp((float)equalizerBand.Amp, (uint)equalizerBand.Number);
        }
        return eq;
    }
}

public enum MediaPlayerState {

    Stopped,
    Playing,
    Paused

}