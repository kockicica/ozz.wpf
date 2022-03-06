using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace ozz.wpf.Services;

public class MainWindowProvider : IMainWindowProvider {

    #region IMainWindowProvider Members

    public Window? GetMainWindow() {
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime l) {
            return l.MainWindow;
        }
        return null;
    }

    #endregion

}