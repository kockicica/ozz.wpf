using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class DataService : IDataService {

    private readonly IHttpClientFactory _clientFactory;

    public DataService(IHttpClientFactory clientFactory) {
        _clientFactory = clientFactory;
    }

    public async Task<IEnumerable<Category>> Categories() {

        var cl = _clientFactory.CreateClient("default");
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/categories");
        using var rsp = await cl.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        var stream = await rsp.Content.ReadAsStringAsync();
        rsp.EnsureSuccessStatusCode();
        var data = JsonConvert.DeserializeObject<IEnumerable<Category>>(stream);
        return data;



    }

    public async Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name) {
        var cl = _clientFactory.CreateClient("default");
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

}