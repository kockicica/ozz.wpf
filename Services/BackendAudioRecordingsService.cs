using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class BackendAudioRecordingsService : IAudioRecordingsService {

    private readonly ILogger<BackendAudioRecordingsService> _logger;
    private readonly IClient                           _client;

    public BackendAudioRecordingsService(ILogger<BackendAudioRecordingsService> logger, IClient client) {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name) {
        return await _client.AudioRecordingsForCategory(categoryId, name);
    }

}