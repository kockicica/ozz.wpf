using System.Reactive;

using Avalonia.Controls;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services.Interactions.Confirm;
using ozz.wpf.ViewModels;
using ozz.wpf.ViewModels.Dialogs;
using ozz.wpf.Views;
using ozz.wpf.Views.Dialogs;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public class OzzInteractions : IOzzInteractions {

    private readonly ILogger<OzzInteractions> _logger;
    private readonly IMainWindowProvider      _mainWindowProvider;
    private readonly IResolver                _resolver;

    public OzzInteractions(ILogger<OzzInteractions> logger, IMainWindowProvider mainWindowProvider, IResolver resolver) {
        _logger = logger;
        _mainWindowProvider = mainWindowProvider;
        _resolver = resolver;

        RegisterHandlers();


    }

    #region IOzzInteractions Members

    public Interaction<BrowseForFileConfig, string>                Browse               { get; } = new();
    public Interaction<AudioRecording, AudioRecording?>            EditAudioRecording   { get; } = new();
    public Interaction<Unit, AudioRecording?>                      CreateAudioRecording { get; } = new();
    public Interaction<ConfirmMessageConfig, ConfirmMessageResult> Confirm              { get; } = new();

    #endregion

    private void RegisterHandlers() {

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

        Confirm.RegisterHandler(async context => {
            var cfg = context.Input;
            var vm = _resolver.GetService<ConfirmDialogViewModel>();
            vm.Message = cfg.Message;
            var modal = new ConfirmDialogWindow { DataContext = vm };
            modal.Title = "Potvrda";
            var res = await modal.ShowDialog<ConfirmDialogResult?>(_mainWindowProvider.GetMainWindow());
            res ??= new ConfirmDialogResult { Result = ConfirmMessageResult.Cancel };

            context.SetOutput(res.Result);
        });

    }
}