using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IClient {

    Task<IEnumerable<Category>>       Categories();
    Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name);
    Task<IEnumerable<Equalizer>?>     Equalizers();
    Task<Equalizer?>                  Equalizer(int id);
    Task<Equalizer?>                  EqualizerByName(string name);
    Task<Equalizer?>                  CreateEqualizer(Equalizer eq);
    Task                              DeleteEqualizer(int id);
    Task<Equalizer?>                  UpdateEqualizer(int id, Equalizer eq);
    Task<User?>                       Authorize(string username);
}