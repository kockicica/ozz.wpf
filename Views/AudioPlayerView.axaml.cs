using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.Controls;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class AudioPlayerView : ReactiveUserControl<AudioPlayerViewModel> {

    //public Slider MS => this.FindControl<Slider>("Slider");
    public PlayerSlider MS => this.FindControl<PlayerSlider>("PlayerSlider");
    
    public AudioPlayerView() {
        InitializeComponent();

        this.WhenActivated(d => {
            Observable.FromEventPattern<ValueChangedEventArgs>(MS, "ValueChanged")
                      .Select(x => x.EventArgs.Value)
                      .Subscribe(y => ViewModel!.Seek(y))
                      .DisposeWith(d);
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
    
}