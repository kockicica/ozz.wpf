using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

using Serilog;

namespace ozz.wpf.Services;

public class RemoteEqualizePresetFactory : IEqualizerPresetFactory {

    private ILogger      _logger;
    private IClient _client;

    public RemoteEqualizePresetFactory(ILogger logger, IClient client) {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<Equalizer>> GetPresets() {
        var data = await _client.Equalizers();
        return data;
    }

    public async Task<Equalizer> GetDefaultPreset() {
        throw new System.NotImplementedException();
    }
}