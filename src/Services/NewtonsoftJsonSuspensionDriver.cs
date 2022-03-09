using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

using Newtonsoft.Json;

using ReactiveUI;

namespace ozz.wpf.Services;

public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver {

    private readonly string                 _fileName;
    private readonly JsonSerializerSettings _settings = new() { TypeNameHandling = TypeNameHandling.All };

    public NewtonsoftJsonSuspensionDriver(string fileName) {
        _fileName = fileName;
    }

    #region ISuspensionDriver Members

    public IObservable<object> LoadState() {
        var lines = File.ReadAllText(_fileName);
        var state = JsonConvert.DeserializeObject<object>(lines, _settings);
        return Observable.Return(state)!;
    }

    public IObservable<Unit> SaveState(object state) {
        var lines = JsonConvert.SerializeObject(state, _settings);
        File.WriteAllText(_fileName, lines);
        return Observable.Return(Unit.Default);
    }

    public IObservable<Unit> InvalidateState() {
        if (File.Exists(_fileName)) {
            File.Delete(_fileName);
        }
        return Observable.Return(Unit.Default);
    }

    #endregion

}