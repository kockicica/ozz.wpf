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

    public async Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp) {
        return await _client.AudioRecordings(sp);
    }

    #endregion

}