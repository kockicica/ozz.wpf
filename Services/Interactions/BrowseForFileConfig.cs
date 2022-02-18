using System.Collections.Generic;

using Avalonia.Controls;

namespace ozz.wpf.Services.Interactions;

public class BrowseForFileConfig {
    public string                 Title         { get; set; }
    public string?                Directory     { get; set; }
    public bool                   AllowMultiple { get; set; }
    public List<FileDialogFilter> Filters       { get; set; }
}