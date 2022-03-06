using ozz.wpf.Dialog;
using ozz.wpf.Models;

namespace ozz.wpf.Views.Disposition;

public class DispositionSelectResult : DialogResultBase {
    public DispositionSelectResult(DispositionSelectItem? item) {
        Item = item;
    }

    public DispositionSelectItem? Item { get; }
}