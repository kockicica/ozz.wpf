using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;

using LibVLCSharp.Shared;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Controls;
using ozz.wpf.Dialog;
using ozz.wpf.Views.Player;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public class DispositionBlockViewModel : DialogViewModelBase {

    private readonly ILogger<DispositionBlockViewModel> _logger;
    private          AudioPlayerConfiguration           _audioPlayerConfiguration;

    private AudioPlayerViewModel _audioPlayerViewModel;

    private MediaList? _blockMedia;

    private Models.Disposition? _currentDisposition;

    private DispositionBlock?                        _dispositionBlock;
    private ObservableCollection<Models.Disposition> _dispositionsToEmit = new();
    private ObservableCollection<Models.Disposition> _emitedDispositions = new();
    private Models.Equalizer                         _equalizer          = Models.Equalizer.Default;
    private bool                                     _isPaused           = true;
    private bool                                     _isStopped          = true;
    private LibVLC                                   _libVLC;
    private long                                     _mediaLength = 1;
    private MediaPlayer                              _mediaPlayer;
    private ObservableAsPropertyHelper<long>         _mediaPosition;

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
        Player.Volume = _volume;

        this.WhenActivated(d => {


            _playerState = Observable.Merge(
                                         Observable.FromEventPattern(_mediaPlayer, "Playing")
                                                   .Select(_ => MediaPlayerState.Playing)
                                                   .Do(state => {
                                                       // IsPaused = false;
                                                       // IsStopped = false;
                                                       //MediaLength = _mediaPlayer.Length;
                                                   }),
                                         Observable.FromEventPattern(_mediaPlayer, "Paused")
                                                   .Select(_ => MediaPlayerState.Paused),
                                         // .Do(state => {
                                         //     IsPaused = true;
                                         //     IsStopped = false;
                                         // }),
                                         Observable.FromEventPattern(_mediaPlayer, "Stopped")
                                                   .Select(_ => MediaPlayerState.Stopped)
                                                   .Do(state => {
                                                       //MediaLength = 1;
                                                       // IsPaused = true;
                                                       // IsStopped = true;
                                                       Player.Volume = Volume;
                                                   })
                                     )
                                     .ToProperty(this, x => x.PlayerState)
                                     .DisposeWith(d);
            // _currentPlayerTime = Observable.Merge(
            //                                    Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, "TimeChanged")
            //                                              .Select(x => TimeSpan.FromMilliseconds(x.EventArgs.Time)),
            //                                    Observable.FromEventPattern(_mediaPlayer, "Stopped").Select(_ => MediaPlayerState.Stopped)
            //                                              .Select(_ => TimeSpan.Zero)
            //                                )
            //                                .ToProperty(this, x => x.CurrentPlayerTime)
            //                                .DisposeWith(d);


            _mediaPosition = Observable.FromEventPattern<MediaPlayerTimeChangedEventArgs>(_mediaPlayer, nameof(MediaPlayer.TimeChanged))
                                       .Select(x => { return EmitedDispositions.Sum(d => d.Duration / 1_000_000) + x.EventArgs.Time; })
                                       .ToProperty(this, x => x.MediaPosition)
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

            //Observable.FromEventPattern<>(_mediaPlayer, M)

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

            EmitedDispositions.Clear();
            if (_dispositionBlock?.Dispositions != null) {
                DispositionsToEmit.AddRange(_dispositionBlock.Dispositions);
            }

            this.RaisePropertyChanged(nameof(Block));

            CurrentDisposition = Next(DispositionsToEmit);

            if (CurrentDisposition != null) {
                Player.Media = FromDisposition(CurrentDisposition!);
            }

            MediaLength = (long)_dispositionBlock.TotalDuration.TotalMilliseconds;
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

    public long MediaLength {
        get => _mediaLength;
        set => this.RaiseAndSetIfChanged(ref _mediaLength, value);
    }

    public long MediaPosition => _mediaPosition?.Value ?? 0;

    public Models.Disposition? CurrentDisposition {
        get => _currentDisposition;
        set {
            this.RaiseAndSetIfChanged(ref _currentDisposition, value);
        }
    }

    public ObservableCollection<Models.Disposition> EmitedDispositions => _emitedDispositions;

    public ObservableCollection<Models.Disposition> DispositionsToEmit => _dispositionsToEmit;

    public void HandlePlay() {
        //Player.Media = _blockMedia.First();
        Player.Volume = Volume;
        Player.Play();
    }

    public void HandlePause() {
        Player.Pause();
    }

    public void HandleStop() {
        Player.Stop();
        Player.Volume = Volume;
    }

    public void HandleSeek(AudioPlayerSeekEventArgs eventArgs) {
        var totalPosition = TimeSpan.FromMilliseconds((double)eventArgs.Position) -
                            TimeSpan.FromMilliseconds(EmitedDispositions.Sum(d => d.Duration / 1_000_000));
        if (totalPosition.TotalMilliseconds < 0) {
            totalPosition = TimeSpan.Zero;
        }
        Player.SeekTo(totalPosition);
    }

    private void HandleMediaPlayerEndReached(EventPattern<EventArgs> d) {

        DispositionsToEmit.Remove(CurrentDisposition!);
        EmitedDispositions.Add(CurrentDisposition!);
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