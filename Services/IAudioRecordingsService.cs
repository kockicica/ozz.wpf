using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IAudioRecordingsService {

    Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp);
    Task<AudioRecording>               Create(CreateAudioRecording data);
    Task<AudioRecording?>              Update(int id, UpdateAudioRecording data);
    Task                               Delete(int id);
}