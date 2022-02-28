using System;
using System.Collections.Generic;
using System.Linq;

namespace ozz.wpf.Views.Disposition;

public class DispositionBlock {

    private List<Models.Disposition> _dispositions = new();

    public TimeSpan TotalDuration => _dispositions.Aggregate(
        TimeSpan.Zero,
        (span, disposition) => span.Add(TimeSpan.FromMilliseconds(disposition.Duration / 1_000_000)));

    public int TotalCount => _dispositions.Count;

    public void HandleDisposition(Models.Disposition d) {

        if (_dispositions.Contains(d)) {
            d.Order = 0;
            _dispositions.Remove(d);
        }
        else {
            _dispositions.Add(d);
            d.Order = _dispositions.Count;
        }
        RenumberDispositions();

    }

    public void Clear() {
        _dispositions.Clear();
    }

    private void RenumberDispositions() {
        var i = 1;
        foreach (var disposition in _dispositions) {
            disposition.Order = i++;
        }

    }
}