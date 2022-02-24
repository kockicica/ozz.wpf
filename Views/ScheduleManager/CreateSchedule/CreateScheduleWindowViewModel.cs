using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateScheduleWindowViewModel : DialogViewModelBase<CreateScheduleWindowResult> {

    private readonly ILogger<CreateScheduleWindowViewModel> _logger;

    private CreateScheduleViewModel _createScheduleViewModel;

    public CreateScheduleWindowViewModel(ILogger<CreateScheduleWindowViewModel> logger, CreateScheduleViewModel createScheduleViewModel) {
        _logger = logger;
        ScheduleViewModel = createScheduleViewModel;
    }

    public CreateScheduleViewModel ScheduleViewModel {
        get => _createScheduleViewModel;
        set => this.RaiseAndSetIfChanged(ref _createScheduleViewModel, value);
    }
}