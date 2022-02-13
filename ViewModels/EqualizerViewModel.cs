using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData.Binding;

using ReactiveUI;

using EqualizerModel = ozz.wpf.Models.Equalizer;

namespace ozz.wpf.ViewModels;

public class EqualizerViewModel : ViewModelBase, IActivatableViewModel {

    private EqualizerModel _equalizer = EqualizerModel.Default;
    private bool           _isUpdating;

    public EqualizerViewModel() {

        ResetCommand = ReactiveCommand.Create(Reset);

        var ps = Equalizer.Bands.Select(b => b.WhenPropertyChanged(band => band.Amp).Select(_ => true)).ToList();
        var ds = Equalizer.WhenPropertyChanged(equalizer => equalizer.PreAmp).Select(_ => true);
        ps.Add(ds);
        EqualizerUpdated = ps.Merge().SkipWhile(_ =>_isUpdating).Throttle(TimeSpan.FromMilliseconds(500)).Select(_ => Equalizer);

        // var ps = Equalizer.Bands.Select(b => b.WhenPropertyChanged(band => band.Amp));
        // var ds = Equalizer.WhenPropertyChanged(equalizer => equalizer.Name);

        //this.WhenActivated(d => {
        // Observable
        //     .Merge(Equalizer.Bands.Select(b => b.WhenAnyPropertyChanged()))
        //     .Subscribe(_ => this.RaisePropertyChanged(nameof(Equalizer)))
        //     .DisposeWith(d);
        //});

    }

    public IObservable<EqualizerModel> EqualizerUpdated { get; set; }

    public ReactiveCommand<Unit, Unit> ResetCommand { get; set; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    public EqualizerModel Equalizer {
        get => _equalizer;
        set {
            UpdateEqualizer(value);
            this.RaisePropertyChanged(nameof(Equalizer));
            //this.RaiseAndSetIfChanged(ref _equalizer, value);
        }
    }

    #endregion

    public void Reset() {
        foreach (var equalizerBand in Equalizer.Bands) {
            equalizerBand.Amp = 0;
        }
        Equalizer.PreAmp = 0;

    }

    private void UpdateEqualizer(EqualizerModel other) {
        if (other == null) {
            return;
        }
        _isUpdating = true;
        _equalizer.Id = other.Id;
        _equalizer.Name = other.Name;
        _equalizer.PreAmp = other.PreAmp;
        foreach (var equalizerBand in _equalizer.Bands) {
            var otherBand = other.Bands.SingleOrDefault(b => b.Number == equalizerBand.Number);
            if (otherBand != null) {
                equalizerBand.Amp = otherBand.Amp;
            }
        }
        _isUpdating = false;
    }

}