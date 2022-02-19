using System;

using ReactiveUI;

namespace ozz.wpf.Services;

public class AudioRecordingsSearchParams : ReactiveObject {
    private bool?     _active;
    private int?      _categoryId;
    private int?      _count;
    private DateTime? _fromDate;
    private string?   _name;
    private int?      _skip;
    private string    _sort;
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

    public string Sort {
        get => _sort;
        set => this.RaiseAndSetIfChanged(ref _sort, value);
    }

    public int? Skip {
        get => _skip;
        set => this.RaiseAndSetIfChanged(ref _skip, value);
    }

    public int? Count {
        get => _count;
        set => this.RaiseAndSetIfChanged(ref _count, value);
    }
}