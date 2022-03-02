using System;
using System.Collections.Generic;
using System.Linq;

namespace ozz.wpf.Views.Disposition;

public class DispositionBlock {

    private List<Models.Disposition> _dispositions = new();

    public TimeSpan TotalDuration => Dispositions.Aggregate(
        TimeSpan.Zero,
        (span, disposition) => span.Add(TimeSpan.FromMilliseconds(disposition.Duration / 1_000_000)));

    public int TotalCount => Dispositions.Count;

    public List<Models.Disposition> Dispositions => _dispositions;

    public void HandleDisposition(Models.Disposition d) {

        if (Dispositions.Contains(d)) {
            d.Order = 0;
            Dispositions.Remove(d);
        }
        else {
            Dispositions.Add(d);
            d.Order = Dispositions.Count;
        }
        RenumberDispositions();

    }

    public void Clear() {
        foreach (var disposition in Dispositions) {
            disposition.Order = 0;
        }
        Dispositions.Clear();
    }

    private void RenumberDispositions() {
        var i = 1;
        foreach (var disposition in Dispositions) {
            disposition.Order = i++;
        }

    }
}