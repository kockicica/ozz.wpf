using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace ozz.wpf.Models;

public class Equalizer : INotifyPropertyChanged {
    private string _name;
    private double _preAmp;

    public int Id { get; set; }

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

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EqualizerBand : INotifyPropertyChanged {
    private double _amp;

    private int _number;

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

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}