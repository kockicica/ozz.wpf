using System.Runtime.Serialization;

using ReactiveUI;

namespace ozz.wpf.Services;

[DataContract]
public class AppState : ReactiveObject {

    private int _volume = 40;

    [DataMember]
    public int Volume {
        get => _volume;
        set => this.RaiseAndSetIfChanged(ref _volume, value);
    }
}