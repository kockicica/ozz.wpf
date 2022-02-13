using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using DynamicData;
using DynamicData.Binding;

using ozz.wpf.Models;

using ReactiveUI;

using EqualizerModel = ozz.wpf.Models.Equalizer;

namespace ozz.wpf.ViewModels;

public class EqualizerViewModel : ViewModelBase, IActivatableViewModel {

    private EqualizerModel          _equalizer        = EqualizerModel.Default;
    private Subject<EqualizerModel> _equalizerUpdated = new();
    private bool                    _isUpdating;

    public EqualizerViewModel() {

        ResetCommand = ReactiveCommand.Create(Reset);

        this.WhenActivated(d => {
            var e = new SourceList<EqualizerBand>();
            e.AddRange(Equalizer.Bands);

            e.Connect()
             .WhenAnyPropertyChanged()
             .SkipWhile(_ => _isUpdating)
             .Subscribe(set => { _equalizerUpdated.OnNext(Equalizer); })
             .DisposeWith(d);

            Equalizer
                .WhenPropertyChanged(x => x.PreAmp)
                .SkipWhile(_ => _isUpdating)
                .Subscribe(_ => _equalizerUpdated.OnNext(Equalizer))
                .DisposeWith(d);

        });

    }

    public IObservable<EqualizerModel> EqualizerUpdated => _equalizerUpdated.AsObservable();

    public ReactiveCommand<Unit, Unit> ResetCommand { get; set; }

    public EqualizerModel Equalizer {
        get => _equalizer;
        set {
            UpdateEqualizer(value);
            this.RaisePropertyChanged(nameof(Equalizer));
            //this.RaiseAndSetIfChanged(ref _equalizer, value);
        }
    }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

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