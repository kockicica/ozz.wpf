using ReactiveUI;

namespace ozz.wpf.Services;

public interface IAppStateManager {
    AppState State { get; }
}

public class AppStateManager : IAppStateManager {

    private AppState _state = RxApp.SuspensionHost.GetAppState<AppState>();

    #region IAppStateManager Members

    public AppState State => _state;

    #endregion

}