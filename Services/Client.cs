using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using ozz.wpf.Models;


namespace ozz.wpf.Services;

public class Client : IClient {

    private readonly ILogger<Client> _logger;
    private readonly HttpClient          _client;

    public Client(ILogger<Client> logger, HttpClient client) {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<Category>> Categories() {

        var cl = _client;
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/categories");
        using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        var stream = await rsp.Content.ReadAsStringAsync();
        rsp.EnsureSuccessStatusCode();
        var data = JsonConvert.DeserializeObject<IEnumerable<Category>>(stream);
        return data;

    }

    public async Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name) {
        var cl = _client;
        var url = $"/api/audio/active/{categoryId}";
        if (!string.IsNullOrEmpty(name)) {
            url += $"?name={name}";
        }
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        var stream = await rsp.Content.ReadAsStringAsync();
        rsp.EnsureSuccessStatusCode();
        var data = JsonConvert.DeserializeObject<IEnumerable<AudioRecording>>(stream);
        return data;
    }

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
            var res = await cl.PostAsJsonAsync(url, eq);
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

}