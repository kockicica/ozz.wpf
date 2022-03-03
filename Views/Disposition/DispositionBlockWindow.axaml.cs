using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using ozz.wpf.Controls;
using ozz.wpf.Dialog;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public partial class DispositionBlockWindow : DialogWindowBase<DialogResultBase> {

    AudioPlayer _audioPlayer;

    public DispositionBlockWindow() {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        _audioPlayer = this.FindControl<AudioPlayer>("AudioPlayer");

        this.WhenActivated(d => {

            Observable.FromEventPattern<RoutedEventArgs>(_audioPlayer, nameof(AudioPlayer.Play))
                      .Subscribe(pattern => {
                          if (ViewModel is DispositionBlockViewModel vm) {
                              vm.HandlePlay();
                          }
                      })
                      .DisposeWith(d);

            Observable.FromEventPattern<RoutedEventArgs>(_audioPlayer, nameof(AudioPlayer.Stop))
                      .Subscribe(pattern => {
                          if (ViewModel is DispositionBlockViewModel vm) {
                              vm.HandleStop();
                          }
                      })
                      .DisposeWith(d);

            Observable.FromEventPattern<RoutedEventArgs>(_audioPlayer, nameof(AudioPlayer.Pause))
                      .Subscribe(pattern => {
                          if (ViewModel is DispositionBlockViewModel vm) {
                              vm.HandlePause();
                          }
                      })
                      .DisposeWith(d);

            Observable.FromEventPattern<AudioPlayerSeekEventArgs>(_audioPlayer, nameof(AudioPlayer.Seek))
                      .Subscribe(pattern => {
                          if (ViewModel is DispositionBlockViewModel vm) {
                              vm.HandleSeek(pattern.EventArgs);
                          }
                      })
                      .DisposeWith(d);

        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}