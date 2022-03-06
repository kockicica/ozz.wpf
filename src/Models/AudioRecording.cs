using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace ozz.wpf.Models;

public class AudioRecording : INotifyPropertyChanged, HasId {
    private bool     _active;
    private string   _category;
    private string   _client;
    private string   _comment;
    private DateTime _date;
    private long     _duration;
    private string   _name;
    private string   _path;

    public string Name {
        get => _name;
        set {
            if (value == _name) return;

            _name = value;
            OnPropertyChanged();
        }
    }

    public string Path {
        get => _path;
        set {
            if (value == _path) return;

            _path = value;
            OnPropertyChanged();
        }
    }

    public string Category {
        get => _category;
        set {
            if (value == _category) return;

            _category = value;
            OnPropertyChanged();
        }
    }

    public long Duration {
        get => _duration;
        set {
            if (value == _duration) return;

            _duration = value;
            OnPropertyChanged();
        }
    }

    public DateTime Date {
        get => _date;
        set {
            if (value.Equals(_date)) return;

            _date = value;
            OnPropertyChanged();
        }
    }

    public bool Active {
        get => _active;
        set {
            if (value == _active) return;

            _active = value;
            OnPropertyChanged();
        }
    }

    public string Client {
        get => _client;
        set {
            if (value == _client) return;

            _client = value;
            OnPropertyChanged();
        }
    }

    public string Comment {
        get => _comment;
        set {
            if (value == _comment) return;

            _comment = value;
            OnPropertyChanged();
        }
    }

    #region HasId Members

    public int Id { get; set; }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class CreateAudioRecording {
    public string   Name     { get; set; }
    public string   Path     { get; set; }
    public string   Category { get; set; }
    public TimeSpan Duration { get; set; }
    public bool     Active   { get; set; }
    public DateTime Date     { get; set; }
    public string   Comment  { get; set; }
    public string   Client   { get; set; }
}

public class UpdateAudioRecording {
    public string Name     { get; set; }
    public string Category { get; set; }
    public string Client   { get; set; }
    public string Comment  { get; set; }
    public bool   Active   { get; set; }
}