using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.Controls;

using ReactiveUI;

namespace ozz.wpf.Views.Player;

public partial class AudioPlayerView : ReactiveUserControl<AudioPlayerViewModel> {

    private Models.Equalizer _equalizer;

    // public static readonly DirectProperty<AudioPlayerView, Equalizer> EqualizerProperty =
    //     AvaloniaProperty.RegisterDirect<AudioPlayerView, Equalizer>(
    //         nameof(Equalizer),
    //         view => view.Equalizer,
    //         defaultBindingMode: BindingMode.OneWay
    //     );
    //
    // public Equalizer Equalizer {
    //     get => _equalizer;
    //     set => _equalizer = value;
    // }

    public AudioPlayerView() {
        InitializeComponent();

        this.WhenActivated(d => {
            Observable.FromEventPattern<ValueChangedEventArgs>(MS, "ValueChanged")
                      .Select(x => x.EventArgs.Value)
                      .Subscribe(y => ViewModel!.Seek(y))
                      .DisposeWith(d);
        });
    }

    //public Slider MS => this.FindControl<Slider>("Slider");
    public PlayerSlider MS => this.FindControl<PlayerSlider>("PlayerSlider");

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}