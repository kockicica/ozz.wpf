using ozz.wpf.Models;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioPlayerViewModel : ViewModelBase {

    private AudioRecording _track;

    public AudioRecording Track {
        get => _track;
        set => this.RaiseAndSetIfChanged(ref _track, value);
    }

}