using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

using ozz.wpf.Models;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsListViewModel : ViewModelBase {

    private ObservableCollection<AudioRecording> _recordings;

    private AudioRecording _selectedRecording;

    public AudioRecordingsListViewModel() {

        ShowPlayer = new Interaction<AudioRecording, Unit>();

        async void Execute(AudioRecording recording) {
            await ShowPlayer.Handle(recording);
        }

        ViewPlayerCommand = ReactiveCommand.Create<AudioRecording>(Execute);

    }

    public ObservableCollection<AudioRecording> Recordings {
        get => _recordings;
        set => _recordings = value;
    }


    public AudioRecording SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public Interaction<AudioRecording, Unit> ShowPlayer { get; }

    public ReactiveCommand<AudioRecording, Unit> ViewPlayerCommand { get; set; }
}