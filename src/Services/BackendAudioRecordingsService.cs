using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class BackendAudioRecordingsService : IAudioRecordingsService {
    private readonly IClient _client;

    private readonly ILogger<BackendAudioRecordingsService> _logger;

    public BackendAudioRecordingsService(ILogger<BackendAudioRecordingsService> logger, IClient client) {
        _logger = logger;
        _client = client;
    }

    #region IAudioRecordingsService Members

    public async Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp, CancellationToken token) {
        return await _client.AudioRecordings(sp, token);
    }

    public async Task<AudioRecording?> Create(CreateAudioRecording data) {
        return await _client.Create(data);
    }

    public async Task<AudioRecording?> Update(int id, UpdateAudioRecording data) {
        return await _client.UpdateAudioRecording(id, data);
    }

    public async Task Delete(int id) {
        await _client.DeleteAudioRecording(id);
    }

    #endregion

}