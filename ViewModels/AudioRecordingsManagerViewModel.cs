using Microsoft.Extensions.Logging;

using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class AudioRecordingsManagerViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel, ICaption {

    private readonly ILogger<AudioRecordingsManagerViewModel> _logger;
    private readonly RoutingState                             _routingState;

    public AudioRecordingsManagerViewModel(ILogger<AudioRecordingsManagerViewModel> logger, IScreen screen, RoutingState routingState) {
        _logger = logger;
        HostScreen = screen;
        _routingState = routingState;
        //this.WhenActivated(d => { });
    }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region ICaption Members

    public string Caption { get; } = "Održavanje audio zapisa";

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = "audio-manager";
    public IScreen HostScreen     { get; }

    #endregion

}