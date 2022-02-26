using System.Collections.ObjectModel;
using System.Reactive;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;

using ReactiveUI;

namespace ozz.wpf.Views.Disposition;

public class DispositionSelectViewModel : DialogViewModelBase<DispositionSelectResult> {

    private readonly ILogger<DispositionSelectViewModel> _logger;

    private DispositionSelectItem? _selected;

    private int? _selectedIndex;

    public DispositionSelectViewModel(ILogger<DispositionSelectViewModel> logger) {
        _logger = logger;

        SelectDisposition = ReactiveCommand.Create(() => { Close(new DispositionSelectResult(Selected)); });

    }

    public ObservableCollection<DispositionSelectItem> Items { get; set; }

    public ReactiveCommand<Unit, Unit> SelectDisposition { get; }

    public DispositionSelectItem? Selected {
        get => _selected;
        set => this.RaiseAndSetIfChanged(ref _selected, value);
    }

    public int? SelectedIndex {
        get => _selectedIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedIndex, value);
    }
}