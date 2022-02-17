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

    private AudioRecordingDetailsViewModel _audioRecordingDetailsViewModel;
    private int                            _id;

    public EditAudioRecordingViewModel(ILogger<EditAudioRecordingViewModel> logger, AudioRecordingDetailsViewModel audioRecordingDetailsViewModel,
                                       IClient client, INotificationManager notificationManager) {
        _logger = logger;
        AudioRecordingDetailsViewModel = audioRecordingDetailsViewModel;
        _client = client;
        _notificationManager = notificationManager;
        Update = ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(async unit => {

            var updateData = new UpdateAudioRecording {
                Active = AudioRecordingDetailsViewModel.Active,
                Category = AudioRecordingDetailsViewModel.SelectedCategory?.Name,
                Client = AudioRecordingDetailsViewModel.Client,
                Comment = AudioRecordingDetailsViewModel.Comment,
                Name = AudioRecordingDetailsViewModel.Name,
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

    public AudioRecordingDetailsViewModel AudioRecordingDetailsViewModel {
        get => _audioRecordingDetailsViewModel;
        set => this.RaiseAndSetIfChanged(ref _audioRecordingDetailsViewModel, value);
    }

    public ReactiveCommand<Unit, AudioRecording?> Update { get; set; }

    public int Id {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
}