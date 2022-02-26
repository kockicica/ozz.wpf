using System;
using System.Reactive;
using System.Reactive.Disposables;

using Avalonia.Controls.Notifications;

using Microsoft.Extensions.Logging;

using ozz.wpf.Dialog;
using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateDispositionViewModel : DialogViewModelBase, IActivatableViewModel {

    private readonly ILogger<CreateDispositionViewModel> _logger;
    private readonly INotificationManager                _notificationManager;
    private readonly IScheduleClient                     _scheduleClient;

    private DateTime? _fromDate     = DateTime.Now;
    private int       _numberOfDays = 7;

    public CreateDispositionViewModel(ILogger<CreateDispositionViewModel> logger, IScheduleClient scheduleClient,
                                      INotificationManager notificationManager) {
        _logger = logger;
        _scheduleClient = scheduleClient;
        _notificationManager = notificationManager;

        Create = ReactiveCommand.CreateFromTask(async token => {
                                                    var cp = new CreateDispositionParams {
                                                        From = FromDate!.Value.ToString("yyyy-MM-dd"),
                                                        Days = NumberOfDays,
                                                    };
                                                    await _scheduleClient.CreateDispositions(cp, token);
                                                    return Unit.Default;
                                                },
                                                this.WhenAnyValue(x => x.FromDate, x => x.NumberOfDays, (dt, nd) => dt != null));

        this.WhenActivated(d => {
            Create
                .Subscribe(_ => Close())
                .DisposeWith(d);
            Create
                .ThrownExceptions
                .Subscribe(exception => {
                    var msg = new Notification("Greška", $"Greška prilikom kreiranja dispozicija:\r\n{exception.Message}");
                    _notificationManager.Show(msg);
                })
                .DisposeWith(d);
        });

    }

    public DateTime? FromDate {
        get => _fromDate;
        set => this.RaiseAndSetIfChanged(ref _fromDate, value);
    }

    public int NumberOfDays {
        get => _numberOfDays;
        set => this.RaiseAndSetIfChanged(ref _numberOfDays, value);
    }

    public ReactiveCommand<Unit, Unit> Create { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

}