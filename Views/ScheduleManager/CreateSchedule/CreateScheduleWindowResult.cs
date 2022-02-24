using System.Collections.Generic;

using ozz.wpf.Dialog;
using ozz.wpf.Models;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateScheduleWindowResult : DialogResultBase {
    private IEnumerable<Schedule>? _schedules;

    public CreateScheduleWindowResult(IEnumerable<Schedule>? schedules) {
        _schedules = schedules;
    }

    public IEnumerable<Schedule>? Schedules => _schedules;
}