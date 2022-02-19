using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class ScheduleClient : IScheduleClient {
    private readonly HttpClient _client;

    private readonly ILogger<ScheduleClient> _logger;
    private readonly IMapper                 _mapper;

    public ScheduleClient(ILogger<ScheduleClient> logger, HttpClient client) {
        _logger = logger;
        _client = client;
    }

    #region IScheduleClient Members

    public async Task<Schedule> Create(CreateScheduleData data, CancellationToken token = default) {
        try {
            var url = $"/api/schedules";
            var rsp = await _client.PostAsJsonAsync(url, data, token);
            if (!rsp.IsSuccessStatusCode) {
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>(cancellationToken: token);
                throw new ScheduleCreateException(msg?.Message);
            }
            return (await rsp.Content.ReadFromJsonAsync<Schedule>(cancellationToken: token))!;

        }
        catch (Exception e) {
            _logger.LogError("Error creating schedule: {@e}", e);
            throw;
        }
    }

    public async Task<IEnumerable<Schedule>> CreateMultiple(IEnumerable<CreateScheduleData> data, CancellationToken token = default) {
        try {
            var url = $"/api/schedules/multiple";
            var rsp = await _client.PostAsJsonAsync(url, data, token);
            if (!rsp.IsSuccessStatusCode) {
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>(cancellationToken: token);
                throw new ScheduleCreateException(msg?.Message);
            }
            return (await rsp.Content.ReadFromJsonAsync<IEnumerable<Schedule>>(cancellationToken: token))!;

        }
        catch (Exception e) {
            _logger.LogError("Error creating multiple schedules: {@e}", e);
            throw;
        }
    }

    public async Task<Schedule> Get(int id, CancellationToken token = default) {
        try {
            var url = $"/api/schedules/{id}";
            var rsp = await _client.GetAsync(url, token);
            if (!rsp.IsSuccessStatusCode) {
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>(cancellationToken: token);
                throw new DatabaseException(msg?.Message);
            }
            return (await rsp.Content.ReadFromJsonAsync<Schedule>(cancellationToken: token))!;
        }
        catch (Exception e) {
            _logger.LogError("Error getting single schedule: {@e}", e);
            throw;
        }
    }

    public async Task<Schedule> Update(int id, Schedule data, CancellationToken token = default) {
        try {
            var url = $"/api/schedules/{id}";
            var rsp = await _client.PostAsJsonAsync(url, data, token);
            if (!rsp.IsSuccessStatusCode) {
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>(cancellationToken: token);
                throw new DatabaseException(msg?.Message);
            }
            return (await rsp.Content.ReadFromJsonAsync<Schedule>(cancellationToken: token))!;
        }
        catch (Exception e) {
            _logger.LogError("Error updating schedule: {@e}", e);
            throw;
        }
    }

    public async Task Delete(int id, CancellationToken? token = null) {
        //throw new NotImplementedException();
        return;
    }

    public async Task<IEnumerable<Schedule>> Find(ScheduleSearchParams sp, CancellationToken token = default) {
        try {
            var url = $"/api/schedules{sp.ToQueryString()}";
            var rsp = await _client.GetAsync(url, token);
            if (!rsp.IsSuccessStatusCode) {
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>(cancellationToken: token);
                throw new DatabaseException(msg?.Message);
            }
            //_mapper.Map<IEnumerable<Schedule>>(await rsp.Content.ReadAsStringAsync());
            return (await rsp.Content.ReadFromJsonAsync<IEnumerable<Schedule>>(cancellationToken: token))!;
        }
        catch (Exception e) {
            _logger.LogError("Error finding schedules: {@e}", e);
            throw;
        }
    }

    #endregion

}

public class ScheduleCreateException : DatabaseException {
    public ScheduleCreateException() {
    }

    protected ScheduleCreateException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public ScheduleCreateException([CanBeNull] string? message) : base(message) {
    }

    public ScheduleCreateException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}