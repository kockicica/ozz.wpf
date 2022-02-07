using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services; 

public class DesignTimeDataService: IDataService {

    public Task<IEnumerable<Category>> Categories() {
        return Task.FromResult(new List<Category> {
            new() { Id = 1, Name = "REKLAME", Order = 1 },
            new() { Id = 2, Name = "ŠPICE", Order = 2 },
            new() { Id = 3, Name = "DŽINGLOVI", Order = 2 },
        }.AsEnumerable());
    }

    public Task<IEnumerable<AudioRecording>> AudioRecordingsForCategory(int categoryId, string name) {
        return Task.FromResult(new List<AudioRecording>().AsEnumerable());
    }

}