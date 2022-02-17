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

    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly ILogger<EditAudioRecordingViewModel> _logger;
    private readonly INotificationManager                 _notificationManager;

    private AudioRecordingDetailsViewModel _audioRecordingDetailsViewModel;
    private int                            _id;
    private bool                           _isUpdate;

    public EditAudioRecordingViewModel(ILogger<EditAudioRecordingViewModel> logger, AudioRecordingDetailsViewModel audioRecordingDetailsViewModel,
                                       INotificationManager notificationManager, IAudioRecordingsService audioRecordingsService) {
        _logger = logger;
        AudioRecordingDetailsViewModel = audioRecordingDetailsViewModel;
        _notificationManager = notificationManager;
        _audioRecordingsService = audioRecordingsService;
        Update = ReactiveCommand.CreateFromTask<Unit, AudioRecording?>(async unit => {

            if (IsUpdate) {
                // perform update
                var updateData = new UpdateAudioRecording {
                    Active = AudioRecordingDetailsViewModel.Active,
                    Category = AudioRecordingDetailsViewModel.SelectedCategory?.Name,
                    Client = AudioRecordingDetailsViewModel.Client,
                    Comment = AudioRecordingDetailsViewModel.Comment,
                    Name = AudioRecordingDetailsViewModel.Name,
                };
                var res = await _audioRecordingsService.Update(Id, updateData);
                return res;
            }
            else {
                // create new
                var createData = new CreateAudioRecording {
                    Active = AudioRecordingDetailsViewModel.Active,
                    Category = AudioRecordingDetailsViewModel.SelectedCategory!.Name,
                    Client = AudioRecordingDetailsViewModel.Client,
                    Comment = AudioRecordingDetailsViewModel.Comment,
                    Date = DateTime.Now,
                    Duration = AudioRecordingDetailsViewModel.Duration,
                    Name = AudioRecordingDetailsViewModel.Name,
                    Path = AudioRecordingDetailsViewModel.FileName,
                };
                var res = await _audioRecordingsService.Create(createData);
                return res;
            }
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

    public bool IsUpdate {
        get => _isUpdate;
        set => this.RaiseAndSetIfChanged(ref _isUpdate, value);
    }
}