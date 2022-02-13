using System.Collections.Generic;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public interface IAudioRecordingsService {
    Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name);
    
}