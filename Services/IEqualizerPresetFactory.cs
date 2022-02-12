using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IEqualizerPresetFactory {

    Task<IEnumerable<Equalizer>> GetPresets();

    Task<Equalizer> GetDefaultPreset();

}