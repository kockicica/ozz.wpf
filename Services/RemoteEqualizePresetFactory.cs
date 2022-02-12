using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

using Serilog;

namespace ozz.wpf.Services;

public class RemoteEqualizePresetFactory : IEqualizerPresetFactory {

    private ILogger      _logger;
    private IDataService _dataService;

    public RemoteEqualizePresetFactory(ILogger logger, IDataService dataService) {
        _logger = logger;
        _dataService = dataService;
    }

    public async Task<IEnumerable<Equalizer>> GetPresets() {
        var data = await _dataService.Equalizers();
        return data;
    }

    public async Task<Equalizer> GetDefaultPreset() {
        throw new System.NotImplementedException();
    }
}