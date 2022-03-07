using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class Client : IClient {
    private readonly HttpClient _client;

    private readonly ILogger<Client> _logger;

    public Client(ILogger<Client> logger, HttpClient client) {
        _logger = logger;
        _client = client;
    }

    #region IClient Members

    public async Task<IEnumerable<Category>> Categories() {

        var cl = _client;
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/categories");
        using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        var stream = await rsp.Content.ReadAsStringAsync();
        rsp.EnsureSuccessStatusCode();
        var data = JsonConvert.DeserializeObject<IEnumerable<Category>>(stream);
        return data;

    }

    // public async Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp, CancellationToken token) {
    //     var cl = _client;
    //     var url = $"/api/audio{ToQueryString(sp)}";
    //
    //     var req = new HttpRequestMessage(HttpMethod.Get, url);
    //     using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
    //     var stream = await rsp.Content.ReadAsStringAsync(token);
    //     rsp.EnsureSuccessStatusCode();
    //     var data = JsonConvert.DeserializeObject<PagedResults<AudioRecording>>(stream);
    //     return data;
    // }
    //
    // public async Task<AudioRecording> Create(CreateAudioRecording data) {
    //     var cl = _client;
    //     var url = $"/api/audio";
    //
    //     try {
    //         await using var inputFile = System.IO.File.OpenRead(data.Path);
    //
    //         var fileName = Path.GetFileName(data.Path);
    //
    //         var content = new MultipartFormDataContent {
    //             { new StreamContent(inputFile), "file", fileName },
    //             { new StringContent(data.Name), "name" },
    //             { new StringContent(data.Category), "category" },
    //             { new StringContent(data.Comment), "comment" },
    //             { new StringContent(data.Active.ToString()), "active" },
    //             { new StringContent(data.Duration.ToString("mm\\mss\\s")), "duration" },
    //         };
    //
    //         using var rsp = await cl.PostAsync(url, content);
    //         if (!rsp.IsSuccessStatusCode) {
    //             // we have some kind of an error
    //             var msg = await rsp.Content.ReadFromJsonAsync<ServerErrorResponse>();
    //             throw new HttpRequestException(msg.Message, null, HttpStatusCode.BadRequest);
    //
    //         }
    //         var res = await rsp.Content.ReadFromJsonAsync<AudioRecording>();
    //         return res;
    //
    //     }
    //     catch (Exception e) {
    //         _logger.LogError("Error creating audio record: {@e}", e);
    //         throw new AudioRecordingCreateException(e.Message);
    //     }
    //
    // }
    //
    // public async Task<AudioRecording?> UpdateAudioRecording(int id, UpdateAudioRecording data) {
    //     var cl = _client;
    //     var url = $"/api/audio/{id}";
    //     try {
    //         var res = await cl.PutAsJsonAsync(url, data);
    //         if (!res.IsSuccessStatusCode) {
    //             var msg = await res.Content.ReadFromJsonAsync<ServerErrorResponse>();
    //             throw new HttpRequestException(msg.Message, null, HttpStatusCode.BadRequest);
    //         }
    //         var ret = await res.Content.ReadFromJsonAsync<AudioRecording>();
    //         return ret;
    //     }
    //     catch (Exception e) {
    //         _logger.LogError("Error updating audio record: {@e}", e);
    //         throw new AudioRecordingUpdateException(e.Message);
    //     }
    // }
    //
    // public async Task DeleteAudioRecording(int id) {
    //     var cl = _client;
    //     var url = $"/api/audio/{id}";
    //     try {
    //         var res = await cl.DeleteAsync(url);
    //         if (!res.IsSuccessStatusCode) {
    //             var msg = await res.Content.ReadFromJsonAsync<ServerErrorResponse>();
    //             throw new HttpRequestException(msg.Message, null, HttpStatusCode.InternalServerError);
    //         }
    //     }
    //     catch (Exception e) {
    //         _logger.LogError("Error updating audio record: {@e}", e);
    //         throw new AudioRecordingDeleteException(e.Message);
    //     }
    // }

    public async Task<IEnumerable<Equalizer>?> Equalizers() {
        var cl = _client;
        var url = $"/api/equalizers";

        try {
            var res = await cl.GetFromJsonAsync<IEnumerable<EqualizerResponse>>(url);
            return res.Select(x => x.ToEqualizer());
        }
        catch (Exception e) {
            _logger.LogError("Error retrieving equalizers: {@e}", e);
            return null;
        }
    }

    public async Task<Equalizer?> Equalizer(int id) {
        var cl = _client;
        var url = $"/api/equalizers/{id}";

        try {
            var res = await cl.GetFromJsonAsync<EqualizerResponse>(url);
            return res.ToEqualizer();
        }
        catch (Exception e) {
            _logger.LogError("Error retrieving equalizers: {@e}", e);
            return null;
        }
    }

    public async Task<Equalizer?> CreateEqualizer(Equalizer eq) {
        var cl = _client;
        var url = $"/api/equalizers";

        try {
            var res = await cl.PostAsJsonAsync(url, EqualizerResponse.FromEqualizer(eq));
            res.EnsureSuccessStatusCode();
            var saved = await res.Content.ReadFromJsonAsync<EqualizerResponse>();
            return saved.ToEqualizer();
        }
        catch (Exception e) {
            _logger.LogError("Error creating equalizer: {@e}", e);
            return null;
        }
    }

    public async Task DeleteEqualizer(int id) {
        var cl = _client;
        var url = $"/api/equalizers/{id}";

        try {
            await cl.DeleteAsync(url);
        }
        catch (Exception e) {
            _logger.LogError("Error updating equalizer: {@e}", e);
        }
    }

    public async Task<Equalizer?> UpdateEqualizer(int id, Equalizer eq) {
        var cl = _client;
        var url = $"/api/equalizers/{id}";

        try {
            var res = await cl.PutAsJsonAsync(url, EqualizerResponse.FromEqualizer(eq));
            res.EnsureSuccessStatusCode();
            var saved = await res.Content.ReadFromJsonAsync<EqualizerResponse>();
            return saved.ToEqualizer();
        }
        catch (Exception e) {
            _logger.LogError("Error updating equalizer: {@e}", e);
            return null;
        }
    }

    public async Task<Equalizer?> EqualizerByName(string name) {
        var cl = _client;
        var url = $"/api/equalizers?name={name}";

        try {
            var res = await cl.GetAsync(url);
            var eq = await res.Content.ReadFromJsonAsync<EqualizerResponse>();
            res.EnsureSuccessStatusCode();
            return eq.ToEqualizer();
        }
        catch (Exception e) {
            _logger.LogError("Error getting equalizer by name: {@e}", e);
            return null;
        }

    }

    public async Task<User?> Authorize(string username) {
        var cl = _client;
        var url = $"/api/authorize";

        try {
            var res = await cl.PostAsJsonAsync(url, new { username });
            var eq = await res.Content.ReadFromJsonAsync<User>();
            res.EnsureSuccessStatusCode();
            return eq;
        }
        catch (Exception e) {
            _logger.LogError("Error authorizing: {@e}", e);
            return null;
        }

    }

    #endregion

    private string ToQueryString(AudioRecordingsSearchParams sp) {
        var items = new List<string>();
        if (sp.Active.HasValue) {
            items.Add($"active={sp.Active.Value}");
        }
        if (!string.IsNullOrEmpty(sp.Name)) {
            items.Add($"name={HttpUtility.UrlEncode(sp.Name)}");
        }
        if (sp.CategoryId.HasValue) {
            items.Add($"category={sp.CategoryId}");
        }
        if (sp.FromDate.HasValue) {
            items.Add($"fromDate={sp.FromDate.Value:yyyy-MM-dd}");
        }
        if (sp.ToDate.HasValue) {
            items.Add($"toDate={sp.ToDate.Value:yyyy-MM-dd}");
        }
        if (sp.Count.HasValue) {
            items.Add($"count={sp.Count}");
        }
        if (sp.Skip.HasValue) {
            items.Add($"skip={sp.Skip}");
        }
        if (!string.IsNullOrEmpty(sp.Sort)) {
            items.Add($"sort={HttpUtility.UrlEncode(sp.Sort)}");
        }
        return items.Any() ? $"?{string.Join("&", items)}" : string.Empty;

    }

    #region Nested type: EqualizerResponse

    private class EqualizerResponse {
        public int    Id     { get; set; }
        public string Name   { get; set; }
        public double PreAmp { get; set; }
        public double Amp1   { get; set; }
        public double Amp2   { get; set; }
        public double Amp3   { get; set; }
        public double Amp4   { get; set; }
        public double Amp5   { get; set; }
        public double Amp6   { get; set; }
        public double Amp7   { get; set; }
        public double Amp8   { get; set; }
        public double Amp9   { get; set; }
        public double Amp10  { get; set; }

        public Equalizer ToEqualizer() {

            var bands = new EqualizerBand[10];
            for (int i = 0; i < 10; i++) {
                bands[i] = new EqualizerBand { Number = i + 1 };
                if (typeof(EqualizerResponse).GetProperty($"Amp{i + 1}") is { } pi) {
                    var val = pi.GetValue(this);
                    if (val != null) {
                        bands[i].Amp = (double)val;
                    }
                }
            }

            return new Equalizer {
                Id = Id,
                Name = Name,
                PreAmp = PreAmp,
                Bands = new ObservableCollection<EqualizerBand>(bands)
            };
        }

        public static EqualizerResponse FromEqualizer(Equalizer eq) {
            var rsp = new EqualizerResponse {
                Id = eq.Id,
                Name = eq.Name,
                PreAmp = eq.PreAmp
            };

            foreach (var equalizerBand in eq.Bands) {
                if (typeof(EqualizerResponse).GetProperty($"Amp{equalizerBand.Number}") is { } pi) {
                    pi.SetValue(rsp, equalizerBand.Amp);
                }
            }
            return rsp;
        }
    }

    #endregion

}

public class OzzzzException : Exception {
    public OzzzzException() {
    }

    protected OzzzzException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public OzzzzException([CanBeNull] string? message) : base(message) {
    }

    public OzzzzException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}

public class DatabaseException : OzzzzException {
    public DatabaseException() {
    }

    protected DatabaseException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public DatabaseException([CanBeNull] string? message) : base(message) {
    }

    public DatabaseException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}

public class AudioRecordingCreateException : DatabaseException {

    public AudioRecordingCreateException() {
    }

    protected AudioRecordingCreateException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public AudioRecordingCreateException([CanBeNull] string? message) : base(message) {
    }

    public AudioRecordingCreateException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}

public class AudioRecordingUpdateException : DatabaseException {
    public AudioRecordingUpdateException() {
    }

    protected AudioRecordingUpdateException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public AudioRecordingUpdateException([CanBeNull] string? message) : base(message) {
    }

    public AudioRecordingUpdateException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}

public class AudioRecordingDeleteException : DatabaseException {
    public AudioRecordingDeleteException() {
    }

    protected AudioRecordingDeleteException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
    }

    public AudioRecordingDeleteException([CanBeNull] string? message) : base(message) {
    }

    public AudioRecordingDeleteException([CanBeNull] string? message, [CanBeNull] Exception? innerException) : base(message, innerException) {
    }
}