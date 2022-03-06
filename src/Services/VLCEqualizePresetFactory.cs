using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class VLCEqualizePresetFactory : IEqualizerPresetFactory {

    private readonly ILogger<VLCEqualizePresetFactory> _logger;

    public VLCEqualizePresetFactory(ILogger<VLCEqualizePresetFactory> logger) {
        _logger = logger;
    }

    #region IEqualizerPresetFactory Members

    public Task<IEnumerable<Equalizer>> GetPresets() {

        var res = new List<Equalizer>();
        var vlceq = new LibVLCSharp.Shared.Equalizer();
        for (uint i = 0; i < vlceq.PresetCount; i++) {
            var peq = new LibVLCSharp.Shared.Equalizer(i);
            var eq = new Equalizer {
                Name = peq.PresetName(i),
                PreAmp = peq.Preamp,
                Bands = new ObservableCollection<EqualizerBand>(
                    new uint[10].Select((ic, idx) => new EqualizerBand { Amp = peq.Amp((uint)idx), Number = idx }))
            };
            res.Add(eq);
        }

        _logger.LogDebug("VLC Presets: {@presets}", res);

        return Task.FromResult(res.AsEnumerable());
    }

    public Task<Equalizer> GetDefaultPreset() {
        return Task.FromResult(Equalizer.Default);
    }

    public Task<Equalizer> SavePreset(Equalizer preset) {
        return Task.FromResult(preset);
    }

    #endregion

}