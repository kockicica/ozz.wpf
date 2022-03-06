using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using Avalonia.Controls;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services.Interactions.Confirm;
using ozz.wpf.Views.AudioManager;
using ozz.wpf.Views.Dialogs;
using ozz.wpf.Views.Disposition;
using ozz.wpf.Views.Equalizer;
using ozz.wpf.Views.Player;
using ozz.wpf.Views.ScheduleManager.CreateSchedule;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public class OzzInteractions : IOzzInteractions {
    private readonly IEqualizerPresetFactory _equalizerPresetFactory;

    private readonly ILogger<OzzInteractions> _logger;
    private readonly IMainWindowProvider      _mainWindowProvider;
    private readonly IResolver                _resolver;

    public OzzInteractions(ILogger<OzzInteractions> logger, IMainWindowProvider mainWindowProvider, IResolver resolver,
                           IEqualizerPresetFactory equalizerPresetFactory) {
        _logger = logger;
        _mainWindowProvider = mainWindowProvider;
        _resolver = resolver;
        _equalizerPresetFactory = equalizerPresetFactory;

        RegisterHandlers();


    }

    #region IOzzInteractions Members

    public Interaction<BrowseForFileConfig, string>                Browse               { get; } = new();
    public Interaction<AudioRecording, AudioRecording?>            EditAudioRecording   { get; } = new();
    public Interaction<Unit, AudioRecording?>                      CreateAudioRecording { get; } = new();
    public Interaction<ConfirmMessageConfig, ConfirmMessageResult> Confirm              { get; } = new();
    public Interaction<AudioRecording, Unit>                       ShowPlayer           { get; } = new();
    public Interaction<Unit, IEnumerable<Schedule>?>               CreateSchedules      { get; } = new();
    public Interaction<Unit, Unit>                                 CreateDispositions   { get; } = new();
    public Interaction<Unit, DispositionSelectItem?>               SelectDisposition    { get; } = new();
    public Interaction<DispositionBlock, Unit>                     ShowBlockPlayer      { get; } = new();

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
            vm.AudioRecordingDetailsViewModel.Active = true;
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
            vm.ButtonTypes = new List<ConfirmButtonType> {
                new() { Button = ConfirmMessageResult.Yes, Class = "alert" },
            };
            var modal = new ConfirmDialogWindow {
                DataContext = vm,
                Title = "Potvrda",
            };
            var res = await modal.ShowDialog<ConfirmDialogResult?>(_mainWindowProvider.GetMainWindow());
            res ??= new ConfirmDialogResult { Result = ConfirmMessageResult.Cancel };

            context.SetOutput(res.Result);
        });

        ShowPlayer.RegisterHandler(HandleShowPlayer);

        CreateSchedules.RegisterHandler(HandleCreateSchedules);

        CreateDispositions.RegisterHandler(async context => {
            var vm = _resolver.GetService<CreateDispositionViewModel>();
            var modal = new CreateDispositionsWindow {
                DataContext = vm,
            };

            await modal.ShowDialog(_mainWindowProvider.GetMainWindow());
            context.SetOutput(Unit.Default);
        });

        SelectDisposition.RegisterHandler(HandleSelectDisposition);
        ShowBlockPlayer.RegisterHandler(HandleShowBlockPlayer);

    }

    private async Task HandleSelectDisposition(InteractionContext<Unit, DispositionSelectItem?> context) {
        var vm = _resolver.GetService<DispositionSelectViewModel>();

        var items = Enumerable.Range(0, 14)
                              .Select(d => Enumerable.Range(1, 4)
                                                     .Select(i => new DispositionSelectItem { Date = DateTime.Now.AddDays(d), Shift = i }))
                              .SelectMany(enumerable => enumerable)
                              .ToList();

        vm.Items = new ObservableCollection<DispositionSelectItem>(items);
        vm.SelectedIndex = 0;

        var modal = new DispositionSelectWindow { DataContext = vm, };
        var res = await modal.ShowDialog<DispositionSelectResult>(_mainWindowProvider.GetMainWindow());
        res ??= new DispositionSelectResult(null);
        context.SetOutput(res.Item);

    }

    private async Task HandleCreateSchedules(InteractionContext<Unit, IEnumerable<Schedule>?> context) {

        var vm = _resolver.GetService<CreateScheduleWindowViewModel>();
        var modal = new CreateScheduleWindow {
            DataContext = vm,
        };

        var res = await modal.ShowDialog<CreateScheduleWindowResult>(_mainWindowProvider.GetMainWindow());
        res ??= new CreateScheduleWindowResult(null);


        context.SetOutput(res.Schedules);
    }

    private async Task HandleShowPlayer(InteractionContext<AudioRecording, Unit> context) {

        var rec = context.Input;

        var vm = _resolver.GetService<ModalAudioPlayerViewModel>();
        var pl = _resolver.GetService<AudioPlayerViewModel>();

        pl.Track = rec;

        vm.PlayerModel = pl;
        vm.AutoPlay = true;
        vm.EqualizerViewModel = new EqualizerViewModel {
            //Equalizer = (await _equalizerPresetFactory.GetPresets()).FirstOrDefault()
        };
        vm.Equalizers = new ObservableCollection<Equalizer>(await _equalizerPresetFactory.GetPresets());
        vm.EqualizerViewModel.Equalizer = await _equalizerPresetFactory.GetDefaultPreset();

        var modal = new ModalAudioPlayerWindow {
            DataContext = vm
        };
        await modal.ShowDialog(_mainWindowProvider.GetMainWindow());

        context.SetOutput(Unit.Default);

    }

    private async Task HandleShowBlockPlayer(InteractionContext<DispositionBlock, Unit> context) {

        var block = context.Input;

        var vm = _resolver.GetService<DispositionBlockViewModel>();
        vm.Block = block;
        var modal = new DispositionBlockWindow {
            DataContext = vm,
        };

        await modal.ShowDialog(_mainWindowProvider.GetMainWindow());
        context.SetOutput(Unit.Default);

        return;
    }
}