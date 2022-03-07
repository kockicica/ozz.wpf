using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views.AudioManager;

public class AudioRecordingsLogViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {
    private readonly IAudioRecordingsService _audioRecordingsService;

    private readonly ILogger<AudioRecordingsLogViewModel> _logger;

    private DateTime? _from;

    private ObservableAsPropertyHelper<IEnumerable<AudioRecordingLog>?> _logs;
    private AudioRecording?                                             _selectedRecording;
    private DateTime?                                                   _to;

    public AudioRecordingsLogViewModel(ILogger<AudioRecordingsLogViewModel> logger, IAudioRecordingsService audioRecordingsService,
                                       IScreen hostScreen) {
        _logger = logger;
        _audioRecordingsService = audioRecordingsService;
        HostScreen = hostScreen;

        Search = ReactiveCommand.CreateFromTask<Unit, IEnumerable<AudioRecordingLog>>(async (_, token) => {
                                                                                          var sp = new AudioRecordingLogSearchParams {
                                                                                              From = From, To = To, Recording = SelectedRecording!.Id
                                                                                          };
                                                                                          return await _audioRecordingsService.Logs(sp, token);
                                                                                      },
                                                                                      this.WhenAny(x => x.SelectedRecording,
                                                                                                   recording => recording.Value != null));

        this.WhenActivated(d => { _logs = Search.ToProperty(this, x => x.Logs).DisposeWith(d); });
    }

    public IEnumerable<AudioRecordingLog>? Logs => _logs?.Value;

    public DateTime? From {
        get => _from;
        set => this.RaiseAndSetIfChanged(ref _from, value);
    }

    public DateTime? To {
        get => _to;
        set => this.RaiseAndSetIfChanged(ref _to, value);
    }

    public AudioRecording? SelectedRecording {
        get => _selectedRecording;
        set => this.RaiseAndSetIfChanged(ref _selectedRecording, value);
    }

    public TimeSpan AutoCompleteThrottleTime { get; } = TimeSpan.FromMilliseconds(500);

    public ReactiveCommand<Unit, IEnumerable<AudioRecordingLog>> Search { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Detaljan log emitovanja";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "audio_recordings_log";
    public IScreen HostScreen     { get; }

    #endregion


    public async Task<IEnumerable<object>> PopulateAsync(string searchText, CancellationToken token) {
        var sp = new AudioRecordingsSearchParams {
            Name = searchText,
            Count = 200,
            Sort = "-Date",
        };
        var res = await _audioRecordingsService.AudioRecordings(sp, token);
        return res.Data.AsEnumerable();
    }
}