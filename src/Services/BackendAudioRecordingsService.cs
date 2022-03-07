using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class BackendAudioRecordingsService : IAudioRecordingsService {
    private readonly HttpClient _client;

    private readonly ILogger<BackendAudioRecordingsService> _logger;

    public BackendAudioRecordingsService(ILogger<BackendAudioRecordingsService> logger, HttpClient client) {
        _logger = logger;
        _client = client;
    }

    #region IAudioRecordingsService Members

    public async Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp, CancellationToken token) {
        var cl = _client;
        var url = $"/api/audio{sp.ToQueryString()}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
        var stream = await rsp.Content.ReadAsStringAsync(token);
        rsp.EnsureSuccessStatusCode();
        var data = JsonConvert.DeserializeObject<PagedResults<AudioRecording>>(stream);
        return data;
    }

    public async Task<AudioRecording?> Create(CreateAudioRecording data) {
        var cl = _client;
        var url = $"/api/audio";

        try {
            await using var inputFile = System.IO.File.OpenRead(data.Path);

            var fileName = Path.GetFileName(data.Path);

            var content = new MultipartFormDataContent {
                { new StreamContent(inputFile), "file", fileName },
                { new StringContent(data.Name), "name" },
                { new StringContent(data.Category), "category" },
                { new StringContent(data.Comment), "comment" },
                { new StringContent(data.Active.ToString()), "active" },
                { new StringContent(data.Duration.ToString("mm\\mss\\s")), "duration" },
            };

            using var rsp = await cl.PostAsync(url, content);
            if (!rsp.IsSuccessStatusCode) {
                // we have some kind of an error
                var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>();
                throw new HttpRequestException(msg.Message, null, HttpStatusCode.BadRequest);

            }
            var res = await rsp.Content.ReadFromJsonAsync<AudioRecording>();
            return res;

        }
        catch (Exception e) {
            _logger.LogError("Error creating audio record: {@e}", e);
            throw new AudioRecordingCreateException(e.Message);
        }

    }

    public async Task<AudioRecording?> Update(int id, UpdateAudioRecording data) {
        var cl = _client;
        var url = $"/api/audio/{id}";
        try {
            var res = await cl.PutAsJsonAsync(url, data);
            if (!res.IsSuccessStatusCode) {
                var msg = await res.Content.ReadFromJsonAsync<ServerErrorResponse>();
                throw new HttpRequestException(msg.Message, null, HttpStatusCode.BadRequest);
            }
            var ret = await res.Content.ReadFromJsonAsync<AudioRecording>();
            return ret;
        }
        catch (Exception e) {
            _logger.LogError("Error updating audio record: {@e}", e);
            throw new AudioRecordingUpdateException(e.Message);
        }
    }

    public async Task Delete(int id) {
        var cl = _client;
        var url = $"/api/audio/{id}";
        try {
            var res = await cl.DeleteAsync(url);
            if (!res.IsSuccessStatusCode) {
                var msg = await res.Content.ReadFromJsonAsync<ServerErrorResponse>();
                throw new HttpRequestException(msg.Message, null, HttpStatusCode.InternalServerError);
            }
        }
        catch (Exception e) {
            _logger.LogError("Error updating audio record: {@e}", e);
            throw new AudioRecordingDeleteException(e.Message);
        }
    }

    #endregion

}