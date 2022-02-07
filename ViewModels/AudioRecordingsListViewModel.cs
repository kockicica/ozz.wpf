using System.Collections.ObjectModel;
using System.Reactive;

using ozz.wpf.Models;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsListViewModel : ViewModelBase {

    private ObservableCollection<AudioRecording> _recordings;

    public AudioRecordingsListViewModel() {
        
    }

    public ObservableCollection<AudioRecording> Recordings {
        get => _recordings;
        set => _recordings = value;
    }

    public ReactiveCommand<AudioRecording, Unit> RecordingClick { get; set; }

}