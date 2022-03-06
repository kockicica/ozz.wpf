using Microsoft.Extensions.Logging;

using ozz.wpf.Services;
using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public class CreateSchedulePageViewModel : ViewModelBase, IRoutableViewModel, IActivatableViewModel, ICaption {

    private readonly ILogger<CreateSchedulePageViewModel> _logger;

    public CreateScheduleViewModel CreateScheduleViewModel;

    public CreateSchedulePageViewModel(ILogger<CreateSchedulePageViewModel> logger, IScreen hostScreen,
                                       CreateScheduleViewModel createScheduleViewModel) {
        _logger = logger;
        HostScreen = hostScreen;
        CreateScheduleViewModel = createScheduleViewModel;
    }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Novi raspored";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "create-page";
    public IScreen HostScreen     { get; }

    #endregion

}