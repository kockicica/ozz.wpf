using System;

using ReactiveUI;

namespace ozz.wpf.Services;

public class AudioRecordingsSearchParams : ReactiveObject {
    private bool?     _active;
    private int?      _categoryId;
    private DateTime? _fromDate;
    private string?   _name;
    private DateTime? _toDate;

    public int? CategoryId {
        get => _categoryId;
        set => this.RaiseAndSetIfChanged(ref _categoryId, value);
    }

    public string? Name {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public DateTime? FromDate {
        get => _fromDate;
        set => this.RaiseAndSetIfChanged(ref _fromDate, value);
    }

    public DateTime? ToDate {
        get => _toDate;
        set => this.RaiseAndSetIfChanged(ref _toDate, value);
    }

    public bool? Active {
        get => _active;
        set => this.RaiseAndSetIfChanged(ref _active, value);
    }
}