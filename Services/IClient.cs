using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IClient {

    Task<IEnumerable<Category>> Categories();

    Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp);

    Task<IEnumerable<Equalizer>?> Equalizers();
    Task<Equalizer?>              Equalizer(int id);
    Task<Equalizer?>              EqualizerByName(string name);
    Task<Equalizer?>              CreateEqualizer(Equalizer eq);
    Task                          DeleteEqualizer(int id);
    Task<Equalizer?>              UpdateEqualizer(int id, Equalizer eq);
    Task<User?>                   Authorize(string username);
}