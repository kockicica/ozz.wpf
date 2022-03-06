using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class RemoteEqualizePresetFactory : IEqualizerPresetFactory {
    private IClient _client;

    private ILogger<RemoteEqualizePresetFactory> _logger;

    public RemoteEqualizePresetFactory(ILogger<RemoteEqualizePresetFactory> logger, IClient client) {
        _logger = logger;
        _client = client;
    }

    #region IEqualizerPresetFactory Members

    public async Task<IEnumerable<Equalizer>> GetPresets() {
        var data = await _client.Equalizers();
        return data;
    }

    public async Task<Equalizer> GetDefaultPreset() {
        var df = await _client.EqualizerByName("default");
        return df ?? Equalizer.Default;
    }

    public async Task<Equalizer> SavePreset(Equalizer preset) {
        if (preset.Id == 0) {
            // create new
            return await _client.CreateEqualizer(preset);
        }
        else {
            // update
            return await _client.UpdateEqualizer(preset.Id, preset);
        }
    }

    #endregion

}