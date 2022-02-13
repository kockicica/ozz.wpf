using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace ozz.wpf.Models;

public class Equalizer : INotifyPropertyChanged {
    private double _preAmp;
    private string _name;

    public int    Id   { get; set; }

    public string Name {
        get => _name;
        set {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public double PreAmp {
        get => _preAmp;
        set {
            _preAmp = value;
            OnPropertyChanged(nameof(PreAmp));
        }
    }

    public ObservableCollection<EqualizerBand> Bands { get; set; }

    public static Equalizer Default {
        get {
            var eq = new Equalizer {
                PreAmp = 0,
                Bands = new ObservableCollection<EqualizerBand>(),
                Name = "default"
            };
            for (int i = 0; i < 10; i++) {
                eq.Bands.Add(new() { Number = i + 1, Amp = 0d });
            }
            return eq;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EqualizerBand : INotifyPropertyChanged {

    private int    _number;
    private double _amp;

    public int Number {
        get => _number;
        set {
            _number = value;
            OnPropertyChanged(nameof(Number));
        }
    }

    public double Amp {
        get => _amp;
        set {
            _amp = value;
            OnPropertyChanged(nameof(Amp));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}