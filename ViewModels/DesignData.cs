using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ozz.wpf.Models;
using ozz.wpf.Services;

using Serilog;

namespace ozz.wpf.ViewModels;

public static class DesignData {

    public static DispositionViewModel DispositionViewModel => new(new DesignTimeDataService(), Log.Logger);

    public static AudioRecordingsListViewModel AudioRecordingsListViewModel => new() {
        Recordings = new ObservableCollection<AudioRecording>(new List<AudioRecording> {
            new() { Id = 1, Name = "Recording 1", Duration = 90000000000, Category = "REKLAME", Date = new DateTime(2022, 2, 5) },
            new() { Id = 2, Name = "Recording 2", Duration = 90000000000, Category = "REKLAME", Date = new DateTime(2022, 2, 5) },
        })
    };

    public static AudioPlayerViewModel AudioPlayerViewModel => new AudioPlayerViewModel {
        Track = new() { Id = 1, Name = "Recording 1", Duration = 90000000000, Category = "REKLAME", Date = new DateTime(2022, 2, 5) },
    };

}