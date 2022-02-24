using System.Reactive;

using Avalonia.Collections;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class ScheduleRecordingViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel {

    private readonly ILogger<ScheduleRecordingViewModel> _logger;
    private          bool                                _isInEdit;
    private          CreateScheduleViewModel             _parent;

    private AudioRecording?        _recording;
    private DataGridCollectionView _schedules;

    public ScheduleRecordingViewModel(ILogger<ScheduleRecordingViewModel> logger, IScreen hostScreen) {
        _logger = logger;
        HostScreen = hostScreen;

        HandleEnterKey = ReactiveCommand.Create(() => { });

    }

    public DataGridCollectionView Schedules {
        get => _schedules;
        set => this.RaiseAndSetIfChanged(ref _schedules, value);
    }

    public ReactiveCommand<Unit, Unit> HandleEnterKey { get; }

    public bool IsInEdit {
        get => _isInEdit;
        set => this.RaiseAndSetIfChanged(ref _isInEdit, value);
    }

    public AudioRecording? Recording {
        get => _recording;
        set => this.RaiseAndSetIfChanged(ref _recording, value);
    }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "schedule-recording";
    public IScreen HostScreen     { get; }

    #endregion

}