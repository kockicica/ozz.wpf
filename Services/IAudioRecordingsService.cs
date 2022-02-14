using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IAudioRecordingsService {
    Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp);
}