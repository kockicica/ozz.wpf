using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IDataService {

    Task<IEnumerable<Category>>       Categories();
    Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name);
    Task<IEnumerable<Equalizer>?>     Equalizers();
    Task<Equalizer?>                  Equalizer(int id);
    Task<Equalizer?>                  CreateEqualizer(Equalizer eq);
    Task                              DeleteEqualizer(int id);
    Task<Equalizer?>                  UpdateEqualizer(int id, Equalizer eq);

}