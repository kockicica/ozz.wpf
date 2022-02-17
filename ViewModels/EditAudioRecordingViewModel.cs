using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.ViewModels;

public class EditAudioRecordingViewModel : DialogViewModelBase<EditAudioRecordingsResult> {
    private readonly IClient _client;

    private readonly ILogger<EditAudioRecordingViewModel> _logger;
    private readonly INotificationManager                 _notificationManager;

    private CreateAudioRecordingViewModel _createAudioRecordingViewModel;
    private int                           _id;

    public EditAudioRecordingViewModel(ILogger<EditAudioRecordingViewModel> logger, CreateAudioRecordingViewModel createAudioRecordingViewModel,
                                       IClient client, INotificationManager notificationManager) {
        _logger = logger;
        CreateAudioRecordingViewModel = createAudioRecordingViewModel;
        _client = client;
        _notificationManager = notificationManager;
        Update = ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(async unit => {

            var updateData = new UpdateAudioRecording {
                Active = CreateAudioRecordingViewModel.Active,
                Category = CreateAudioRecordingViewModel.SelectedCategory?.Name,
                Client = CreateAudioRecordingViewModel.Client,
                Comment = CreateAudioRecordingViewModel.Comment,
                Name = CreateAudioRecordingViewModel.Name,
            };
            var res = await _client.UpdateAudioRecording(Id, updateData);
            return res;
        });

        this.WhenActivated(d => {
            Update
                .Where(x => x != null)
                .Subscribe(recording => { this.Close(new() { Recording = recording }); })
                .DisposeWith(d);

            Update
                .ThrownExceptions
                .Where(x => x is AudioRecordingUpdateException)
                .Subscribe(exception => { _notificationManager.Show(new Notification("Greška", exception.Message, NotificationType.Error)); })
                .DisposeWith(d);

        });
    }

    public CreateAudioRecordingViewModel CreateAudioRecordingViewModel {
        get => _createAudioRecordingViewModel;
        set => this.RaiseAndSetIfChanged(ref _createAudioRecordingViewModel, value);
    }

    public ReactiveCommand<Unit, AudioRecording?> Update { get; set; }

    public int Id {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
}