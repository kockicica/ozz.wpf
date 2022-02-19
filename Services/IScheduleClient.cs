using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IScheduleClient {

    Task<Schedule>              Create(CreateScheduleData data, CancellationToken token = default);
    Task<IEnumerable<Schedule>> CreateMultiple(IEnumerable<CreateScheduleData> data, CancellationToken token = default);
    Task<Schedule>              Get(int id, CancellationToken token = default);
    Task<Schedule>              Update(int id, Schedule data, CancellationToken token = default);
    Task                        Delete(int id, CancellationToken? token = null);
    Task<IEnumerable<Schedule>> Find(ScheduleSearchParams sp, CancellationToken token = default);
}