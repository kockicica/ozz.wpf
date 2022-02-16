using System.Collections.Generic;

using Avalonia.Controls;

using Microsoft.Extensions.Logging;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public class BrowseForFileConfig {
    public string                 Title         { get; set; }
    public string?                Directory     { get; set; }
    public bool                   AllowMultiple { get; set; }
    public List<FileDialogFilter> Filters       { get; set; }
}

public interface IBrowseForFile {

    Interaction<BrowseForFileConfig, string> Browse { get; }
}

public class BrowseForFile : IBrowseForFile {

    private readonly ILogger<BrowseForFile> _logger;
    private readonly IMainWindowProvider    _mainWindowProvider;

    public BrowseForFile(ILogger<BrowseForFile> logger, IMainWindowProvider mainWindowProvider) {
        _logger = logger;
        _mainWindowProvider = mainWindowProvider;
        Browse.RegisterHandler(async context => {
            var cfg = context.Input;
            var dlg = new OpenFileDialog { Title = cfg.Title, Directory = cfg.Directory, AllowMultiple = cfg.AllowMultiple, Filters = cfg.Filters };
            var res = await dlg.ShowAsync(_mainWindowProvider.GetMainWindow()!);
            context.SetOutput(res?[0]);
        });

    }

    #region IBrowseForFile Members

    public Interaction<BrowseForFileConfig, string> Browse { get; } = new();

    #endregion

}