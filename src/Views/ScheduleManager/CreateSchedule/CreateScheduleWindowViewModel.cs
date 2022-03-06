using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateScheduleWindowViewModel : DialogViewModelBase<CreateScheduleWindowResult> {

    private readonly ILogger<CreateScheduleWindowViewModel> _logger;
    private readonly IScheduleClient                        _scheduleClient;

    private CreateScheduleViewModel _createScheduleViewModel;

    public CreateScheduleWindowViewModel(ILogger<CreateScheduleWindowViewModel> logger, CreateScheduleViewModel createScheduleViewModel,
                                         IScheduleClient scheduleClient) {
        _logger = logger;
        ScheduleViewModel = createScheduleViewModel;
        _scheduleClient = scheduleClient;

        Create = ReactiveCommand.CreateFromTask(async token => {
            var schedules = ScheduleViewModel.ScheduleRecordingViewModel!.Schedules.Cast<Schedule>().ToList();
            if (schedules.Any()) {
                var createScheduleData = schedules.Select(s => new CreateScheduleData {
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    Recording = s.Recording.Id,
                    Shift1 = s.Shift1,
                    Shift2 = s.Shift2,
                    Shift3 = s.Shift3,
                    Shift4 = s.Shift4,
                    TotalPlayCount = s.TotalPlayCount,
                });
                return await _scheduleClient.CreateMultiple(createScheduleData, token);
            }
            return null;
        });

        this.WhenActivated(d => { Create.Subscribe(schedules => Close(new CreateScheduleWindowResult(schedules))).DisposeWith(d); });

    }

    public CreateScheduleViewModel ScheduleViewModel {
        get => _createScheduleViewModel;
        set => this.RaiseAndSetIfChanged(ref _createScheduleViewModel, value);
    }

    public ReactiveCommand<Unit, IEnumerable<Schedule>?> Create { get; }
}