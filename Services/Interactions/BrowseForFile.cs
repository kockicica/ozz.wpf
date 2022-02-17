using System.Collections.Generic;
using System.Reactive;

using Avalonia.Controls;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.ViewModels;
using ozz.wpf.Views;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public class BrowseForFileConfig {
    public string                 Title         { get; set; }
    public string?                Directory     { get; set; }
    public bool                   AllowMultiple { get; set; }
    public List<FileDialogFilter> Filters       { get; set; }
}

public interface IBrowseForFile {

    Interaction<BrowseForFileConfig, string>     Browse               { get; }
    Interaction<AudioRecording, AudioRecording?> EditAudioRecording   { get; }
    Interaction<Unit, AudioRecording?>           CreateAudioRecording { get; }
}

public class BrowseForFile : IBrowseForFile {
    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly ILogger<BrowseForFile> _logger;
    private readonly IMainWindowProvider    _mainWindowProvider;
    private readonly IResolver              _resolver;

    public BrowseForFile(ILogger<BrowseForFile> logger, IMainWindowProvider mainWindowProvider, IAudioRecordingsService audioRecordingsService,
                         IResolver resolver) {
        _logger = logger;
        _mainWindowProvider = mainWindowProvider;
        _audioRecordingsService = audioRecordingsService;
        _resolver = resolver;

        Browse.RegisterHandler(async context => {
            var cfg = context.Input;
            var dlg = new OpenFileDialog { Title = cfg.Title, Directory = cfg.Directory, AllowMultiple = cfg.AllowMultiple, Filters = cfg.Filters };
            var res = await dlg.ShowAsync(_mainWindowProvider.GetMainWindow()!);
            context.SetOutput(res?[0]);
        });

        EditAudioRecording.RegisterHandler(async context => {

            var rec = context.Input;

            var vm = _resolver.GetService<EditAudioRecordingViewModel>();
            vm.AudioRecordingDetailsViewModel = _resolver.GetService<AudioRecordingDetailsViewModel>();
            vm.AudioRecordingDetailsViewModel.Name = rec.Name;
            vm.AudioRecordingDetailsViewModel.Active = rec.Active;
            vm.AudioRecordingDetailsViewModel.Comment = rec.Comment;
            vm.AudioRecordingDetailsViewModel.Client = rec.Client;
            vm.AudioRecordingDetailsViewModel.IsUpdate = true;
            vm.AudioRecordingDetailsViewModel.CategoryName = rec.Category;
            vm.Id = rec.Id;
            //vm.Category = rec.Category;
            vm.AudioRecordingDetailsViewModel.AudioPlayerViewModel.Track = rec;
            vm.IsUpdate = true;

            var modal = new EditAudioRecordingWindow {
                DataContext = vm
            };
            modal.Title = "Izmena podataka audio zapisa";

            var res = await modal.ShowDialog<EditAudioRecordingsResult>(_mainWindowProvider.GetMainWindow());
            context.SetOutput(res?.Recording);

        });

        CreateAudioRecording.RegisterHandler(async context => {
            var vm = _resolver.GetService<EditAudioRecordingViewModel>();
            vm.AudioRecordingDetailsViewModel = _resolver.GetService<AudioRecordingDetailsViewModel>();
            var modal = new EditAudioRecordingWindow {
                DataContext = vm
            };
            modal.Title = "Kreiranje novog audio zapisa";

            var res = await modal.ShowDialog<EditAudioRecordingsResult>(_mainWindowProvider.GetMainWindow());
            context.SetOutput(res?.Recording);
        });

    }

    #region IBrowseForFile Members

    public Interaction<BrowseForFileConfig, string>     Browse               { get; } = new();
    public Interaction<AudioRecording, AudioRecording?> EditAudioRecording   { get; } = new();
    public Interaction<Unit, AudioRecording?>           CreateAudioRecording { get; } = new();

    #endregion

}