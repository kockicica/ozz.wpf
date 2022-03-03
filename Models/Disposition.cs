using System;

namespace ozz.wpf.Models;

public class DispositionSelectItem {
    public DateTime Date  { get; set; }
    public int      Shift { get; set; }
}

public class CreateDispositionParams {
    public string From { get; set; }
    public int    Days { get; set; }
}

public class DispositionSearchParams {
    public DateTime Date  { get; set; }
    public int      Shift { get; set; }
}

public class Disposition : AudioRecording {
    private int _order;
    private int _playCountCurrent;
    private int _playCountNeeded;
    private int _playCountRemaining;
    private int _scheduleId;
    private int _shift;

    public int Shift {
        get => _shift;
        set {
            if (value == _shift) return;

            _shift = value;
            OnPropertyChanged();
        }
    }

    public int PlayCountNeeded {
        get => _playCountNeeded;
        set {
            if (value == _playCountNeeded) return;

            _playCountNeeded = value;
            OnPropertyChanged();
        }
    }

    public int PlayCountCurrent {
        get => _playCountCurrent;
        set {
            if (value == _playCountCurrent) return;

            _playCountCurrent = value;
            OnPropertyChanged();
        }
    }

    public int PlayCountRemaining {
        get => _playCountRemaining;
        set {
            if (value == _playCountRemaining) return;

            _playCountRemaining = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PlayCountRemainingAsString));
        }
    }

    public int ScheduleId {
        get => _scheduleId;
        set {
            if (value == _scheduleId) return;

            _scheduleId = value;
            OnPropertyChanged();
        }
    }

    public int Order {
        get => _order;
        set {
            if (value == _order) return;

            _order = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(OrderAsString));
        }
    }

    public string PlayCountRemainingAsString => PlayCountRemaining == 0 ? "" : $"X{PlayCountRemaining}";

    public string OrderAsString => Order == 0 ? "" : $"{Order}";

    public void IncreasePlayCount() {
        if (PlayCountCurrent < PlayCountNeeded) {
            PlayCountCurrent += 1;
            PlayCountRemaining -= 1;
        }
    }
}