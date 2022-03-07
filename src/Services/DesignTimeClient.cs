using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public class DesignTimeClient : IClient {

    #region IClient Members

    public Task<IEnumerable<Category>> Categories() {
        return Task.FromResult(new List<Category> {
            new() { Id = 1, Name = "REKLAME", Order = 1 },
            new() { Id = 2, Name = "ŠPICE", Order = 2 },
            new() { Id = 3, Name = "DŽINGLOVI", Order = 2 },
        }.AsEnumerable());
    }

    public Task<IEnumerable<Equalizer>?> Equalizers() {
        return Task.FromResult(new List<Equalizer>().AsEnumerable())!;
    }

    public async Task<Equalizer?> Equalizer(int id) {
        throw new System.NotImplementedException();
    }

    public async Task<Equalizer?> CreateEqualizer(Equalizer eq) {
        throw new System.NotImplementedException();
    }

    public Task DeleteEqualizer(int id) {
        return Task.CompletedTask;
    }

    public async Task<Equalizer?> UpdateEqualizer(int id, Equalizer eq) {
        throw new System.NotImplementedException();
    }

    public async Task<Equalizer?> EqualizerByName(string name) {
        throw new System.NotImplementedException();
    }

    public async Task<User?> Authorize(string username) {
        throw new System.NotImplementedException();
    }

    #endregion

    public Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp, CancellationToken token) {
        return Task.FromResult(new PagedResults<AudioRecording> { Count = 0, Data = System.Array.Empty<AudioRecording>().AsEnumerable() });
    }

    public async Task<AudioRecording> Create(CreateAudioRecording data) {
        throw new System.NotImplementedException();
    }

    public async Task<AudioRecording?> UpdateAudioRecording(int id, UpdateAudioRecording data) {
        throw new System.NotImplementedException();
    }

    public async Task DeleteAudioRecording(int id) {
        throw new System.NotImplementedException();
    }
}

public class DesignTimeAudioRecordingsService : IAudioRecordingsService {

    #region IAudioRecordingsService Members

    public Task<PagedResults<AudioRecording>> AudioRecordings(AudioRecordingsSearchParams sp, CancellationToken token) {
        return Task.FromResult(new PagedResults<AudioRecording>() { Count = 0, Data = System.Array.Empty<AudioRecording>().AsEnumerable() });
    }

    public Task<AudioRecording> Create(CreateAudioRecording data) {
        return Task.FromResult(new AudioRecording());
    }

    public Task<AudioRecording?> Update(int id, UpdateAudioRecording data) {
        return Task.FromResult(new AudioRecording())!;
    }

    public Task Delete(int id) {
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AudioRecordingLog>> Logs(AudioRecordingLogSearchParams sp, CancellationToken token = default) {
        return Task.FromResult(new List<AudioRecordingLog>().AsEnumerable());
    }

    #endregion

}