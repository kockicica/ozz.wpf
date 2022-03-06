using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

using JetBrains.Annotations;

using ozz.wpf.Models;
using ozz.wpf.Views.Player;

namespace ozz.wpf.Controls;

public class AudioPlayerSeekEventArgs : RoutedEventArgs {

    public AudioPlayerSeekEventArgs(long? position) {
        Position = position;
    }

    public AudioPlayerSeekEventArgs([CanBeNull] RoutedEvent? routedEvent, long? position) : base(routedEvent) {
        Position = position;
    }

    public AudioPlayerSeekEventArgs([CanBeNull] RoutedEvent? routedEvent, [CanBeNull] IInteractive? source, long? position) : base(
        routedEvent,
        source) {
        Position = position;
    }

    public long? Position { get; }
}

public class AudioPlayer : TemplatedControl {

    public static readonly DirectProperty<AudioPlayer, AudioRecording?> TrackProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, AudioRecording?>(nameof(Track),
                                                                      player => player.Track,
                                                                      (player, recording) => player.Track = recording);

    public static readonly DirectProperty<AudioPlayer, string?> TrackNameProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, string?>(nameof(TrackName),
                                                              player => player.TrackName,
                                                              (player, n) => player.TrackName = n);

    public static readonly DirectProperty<AudioPlayer, long?> DurationProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, long?>(nameof(Duration),
                                                            player => player.Duration,
                                                            (player, n) => player.Duration = n);

    public static readonly DirectProperty<AudioPlayer, int> VolumeProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, int>(nameof(Volume),
                                                          player => player.Volume,
                                                          (player, v) => player.Volume = v,
                                                          unsetValue: 0,
                                                          BindingMode.TwoWay);

    private static readonly DirectProperty<AudioPlayer, TimeSpan?> CurrentTimeProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, TimeSpan?>(nameof(CurrentTime),
                                                                player => player.CurrentTime,
                                                                (player, v) => player.CurrentTime = v);

    public static readonly DirectProperty<AudioPlayer, long?> MediaLengthProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, long?>(nameof(MediaLength),
                                                            player => player.MediaLength,
                                                            (player, v) => player.MediaLength = v);

    public static readonly DirectProperty<AudioPlayer, long?> MediaPositionProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, long?>(nameof(MediaPosition),
                                                            player => player.MediaPosition,
                                                            (player, v) => player.MediaPosition = v);

    public static readonly DirectProperty<AudioPlayer, MediaPlayerState> PlayerStateProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, MediaPlayerState>(nameof(PlayerState),
                                                                       player => player.PlayerState,
                                                                       (player, v) => player.PlayerState = v);

    public static readonly DirectProperty<AudioPlayer, bool?> CanStopProperty =
        AvaloniaProperty.RegisterDirect<AudioPlayer, bool?>(nameof(CanStop),
                                                            player => player.CanStop,
                                                            (player, v) => player.CanStop = v);



    public static readonly RoutedEvent<RoutedEventArgs> PlayEvent =
        RoutedEvent.Register<AudioPlayer, RoutedEventArgs>(nameof(Play), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> PauseEvent =
        RoutedEvent.Register<AudioPlayer, RoutedEventArgs>(nameof(Pause), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> StopEvent =
        RoutedEvent.Register<AudioPlayer, RoutedEventArgs>(nameof(Stop), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<AudioPlayerSeekEventArgs> SeekEvent =
        RoutedEvent.Register<AudioPlayer, AudioPlayerSeekEventArgs>(nameof(Seek), RoutingStrategies.Bubble);

    private bool? _canStop;

    private TimeSpan? _currentTime;

    private long? _duration;

    private TextBlock? _durationBlock;
    private Equalizer? _equalizer;
    private PathIcon?  _iconPause;
    private PathIcon?  _iconPlay;

    private PathIcon? _iconStop;

    private long? _mediaLength;
    private long? _mediaPosition;

    private PlayerSlider?    _playerSlider;
    private MediaPlayerState _playerState = MediaPlayerState.Stopped;
    private Button?          _playOrPauseButton;

    private IDisposable _playOrPauseButtonSub;
    private Button?     _stopButton;
    private IDisposable _stopButtonSub;

    private AudioRecording? _track;

    private string? _trackName;
    private int     _volume;

    private Slider? _volumeSlider;

    static AudioPlayer() {
        IsEnabledProperty.OverrideDefaultValue<AudioPlayer>(false);
    }

    public string? TrackName {
        get => _trackName;
        set => SetAndRaise(TrackNameProperty, ref _trackName, value);
    }

    public long? Duration {
        get => _duration;
        set => SetAndRaise(DurationProperty, ref _duration, value);
    }

    public AudioRecording? Track {
        get => _track;
        set {
            SetAndRaise(TrackProperty, ref _track, value);
            SetValue(IsEnabledProperty, _track != null);
        }
    }

    public int Volume {
        get => _volume;
        set => SetAndRaise(VolumeProperty, ref _volume, value);
    }

    private TimeSpan? CurrentTime {
        get => _currentTime;
        set => SetAndRaise(CurrentTimeProperty, ref _currentTime, value);
    }

    public long? MediaLength {
        get => _mediaLength;
        set => SetAndRaise(MediaLengthProperty, ref _mediaLength, value);
    }

    public long? MediaPosition {
        get => _mediaPosition;
        set => SetAndRaise(MediaPositionProperty, ref _mediaPosition, value);
    }

    public MediaPlayerState PlayerState {
        get => _playerState;
        set {
            SetAndRaise(PlayerStateProperty, ref _playerState, value);
            HandlePlayerStateChange();
        }
    }

    public bool? CanStop {
        get => _canStop;
        set => SetAndRaise(CanStopProperty, ref _canStop, value);
    }

    public event EventHandler<RoutedEventArgs> Play {
        add => AddHandler(PlayEvent, value);
        remove => RemoveHandler(PlayEvent, value);
    }

    public event EventHandler<RoutedEventArgs> Pause {
        add => AddHandler(PauseEvent, value);
        remove => RemoveHandler(PauseEvent, value);
    }

    public event EventHandler<RoutedEventArgs> Stop {
        add => AddHandler(StopEvent, value);
        remove => RemoveHandler(StopEvent, value);
    }

    public event EventHandler<AudioPlayerSeekEventArgs> Seek {
        add => AddHandler(SeekEvent, value);
        remove => RemoveHandler(SeekEvent, value);
    }


    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
        base.OnPropertyChanged(change);


        if (change.Property == MediaPositionProperty && change.NewValue is BindingValue<long?> { HasValue: true } v) {
            var ts = TimeSpan.FromMilliseconds((double)v.Value!);
            CurrentTime = ts;
        }

        if (change.Property == PlayerStateProperty && change.NewValue is BindingValue<MediaPlayerState?> { HasValue: true } mps) {
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        if (_stopButton is { }) {
            _stopButton = null;
            _stopButtonSub?.Dispose();
        }
        _stopButton = e.NameScope.Find<Button>("PART_Stop");
        _stopButtonSub = _stopButton.AddDisposableHandler(Button.ClickEvent, HandleStopButtonClick);

        if (_playOrPauseButton is { }) {
            _playOrPauseButton = null;
            _playOrPauseButtonSub?.Dispose();
        }
        _playOrPauseButton = e.NameScope.Find<Button>("PART_PlayOrPause");
        _playOrPauseButtonSub = _playOrPauseButton.AddDisposableHandler(Button.ClickEvent, HandlePlayOrPauseButtonClick);

        if (_iconStop is { }) {
            _iconStop = null;
        }
        _iconStop = e.NameScope.Find<PathIcon>("PART_IconStop");

        if (_iconPause is { }) {
            _iconPause = null;
        }
        _iconPause = e.NameScope.Find<PathIcon>("PART_IconPause");

        if (_iconPlay is { }) {
            _iconPlay = null;
        }
        _iconPlay = e.NameScope.Find<PathIcon>("PART_IconPlay");

        if (_volumeSlider is { }) {
            _volumeSlider = null;
        }
        _volumeSlider = e.NameScope.Find<Slider>("VolumeSlider");

        _volumeSlider.PropertyChanged += OnVolumeSliderOnPropertyChanged;

        _playerSlider = e.NameScope.Find<PlayerSlider>("PlayerSlider");

        _playerSlider.ValueChanged += OnPlayerSliderOnValueChanged;

        _durationBlock = e.NameScope.Find<TextBlock>("PART_Duration");


        HandlePlayerStateChange();

    }


    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        if (_playerSlider != null) {
            _playerSlider.ValueChanged -= OnPlayerSliderOnValueChanged;
        }
        if (_volumeSlider != null) {
            _volumeSlider.PropertyChanged -= OnVolumeSliderOnPropertyChanged;
        }
    }

    // private void OnPlayerSliderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args) {
    //     if (args.Property == PlayerSlider.OutsideValueProperty && args.NewValue != null) {
    //         var d = Convert.ToInt64(args.NewValue);
    //         RaiseEvent(new AudioPlayerSeekEventArgs(SeekEvent, d));
    //     }
    // }

    private void OnVolumeSliderOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args) {
        if (args.Property == RangeBase.ValueProperty && args.NewValue != null) {
            var volume = Convert.ToInt32(args.NewValue);
            Volume = volume;
        }
    }

    private void OnPlayerSliderOnValueChanged(object? sender, ValueChangedEventArgs e) {
        var d = Convert.ToInt64(e.Value);
        RaiseEvent(new AudioPlayerSeekEventArgs(SeekEvent, d));

    }

    private void HandleStopButtonClick(object? sender, RoutedEventArgs args) {
        RaiseEvent(new RoutedEventArgs(StopEvent));
    }

    private void HandlePlayOrPauseButtonClick(object? o, RoutedEventArgs routedEventArgs) {
        switch (PlayerState) {
            case MediaPlayerState.Paused:
            case MediaPlayerState.Stopped:
                RaiseEvent(new RoutedEventArgs(PlayEvent));
                break;
            case MediaPlayerState.Playing:
                RaiseEvent(new RoutedEventArgs(PauseEvent));
                break;
        }
    }


    private void HandlePlayerStateChange() {
        switch (PlayerState) {
            case MediaPlayerState.Paused:
                if (_stopButton != null) {
                    _stopButton.IsEnabled = true;
                }
                if (_playOrPauseButton != null) {
                    _playOrPauseButton.IsEnabled = true;
                }
                if (_iconPlay != null) {
                    _iconPlay.IsVisible = true;
                }
                if (_iconPause != null) {
                    _iconPause.IsVisible = false;
                }
                break;
            case MediaPlayerState.Stopped:
                if (_stopButton != null) {
                    _stopButton.IsEnabled = false;
                }
                if (_playOrPauseButton != null) {
                    _playOrPauseButton.IsEnabled = true;
                }
                if (_iconPlay != null) {
                    _iconPlay.IsVisible = true;
                }
                if (_iconPause != null) {
                    _iconPause.IsVisible = false;
                }
                break;
            case MediaPlayerState.Playing:
                if (_stopButton != null) {
                    _stopButton.IsEnabled = true;
                }
                if (_playOrPauseButton != null) {
                    _playOrPauseButton.IsEnabled = true;
                }
                if (_iconPlay != null) {
                    _iconPlay.IsVisible = false;
                }
                if (_iconPause != null) {
                    _iconPause.IsVisible = true;
                }
                break;
        }

    }
}