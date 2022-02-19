using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using ozz.wpf.Config;
using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.Views.Dialogs;
using ozz.wpf.Views.Equalizer;
using ozz.wpf.Views.Manager;
using ozz.wpf.Views.Player;

namespace ozz.wpf.ViewModels;

public static class DesignData {

    public static DispositionViewModel DispositionViewModel
        => new(new DesignTimeClient(),
               NullLogger<DialogWindowViewModel>.Instance,
               new VLCEqualizePresetFactory(NullLogger<VLCEqualizePresetFactory>.Instance),
               new LocatorBasedResolver(),
               new DesignTimeAudioRecordingsService(),
               null);

    public static AudioPlayerViewModel AudioPlayerViewModel
        => new(NullLogger<AudioPlayerViewModel>.Instance,
               new OptionsWrapper<AudioPlayerConfiguration>(new AudioPlayerConfiguration()),
               new OptionsWrapper<ServerConfiguration>(new ServerConfiguration())) {
            //Track = new() { Id = 1, Name = "Recording 1", Duration = 90000000000, Category = "REKLAME", Date = new DateTime(2022, 2, 5) },
        };

    public static EqualizerViewModel Equalizer => new() {
        Equalizer = new() {
            PreAmp = 0.2d,
            Name = "Test preset",
            Bands = new ObservableCollection<EqualizerBand>(new EqualizerBand[] {
                new() { Amp = 1.1, Number = 1 },
                new() { Amp = 2.2d, Number = 2 },
                new() { Amp = -2.2d, Number = 3 },
                new() { Amp = -4.34d, Number = 4 },
                new() { Amp = 2.13d, Number = 5 },
                new() { Amp = 1.34d, Number = 6 },
                new() { Amp = 2.2d, Number = 7 },
                new() { Amp = 2.2d, Number = 8 },
                new() { Amp = 2.2d, Number = 9 },
                new() { Amp = 2.2d, Number = 10 },
            })
        }
    };

    public static ModalAudioPlayerViewModel ModalAudioPlayer
        => new(NullLogger<ModalAudioPlayerViewModel>.Instance, new VLCEqualizePresetFactory(NullLogger<VLCEqualizePresetFactory>.Instance))
            { EqualizerViewModel = Equalizer, PlayerModel = AudioPlayerViewModel };

    public static AudioRecordingsManagerViewModel AudioRecordingsManagerViewModel
        => new(NullLogger<AudioRecordingsManagerViewModel>.Instance, null, new DesignTimeClient(), new DesignTimeAudioRecordingsService(), null);

    public static ManagerViewModel ManagerViewModel => new(NullLogger<ManagerViewModel>.Instance, null, new LocatorBasedResolver(), null, null);
}