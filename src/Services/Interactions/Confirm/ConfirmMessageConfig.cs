using System.Collections.Generic;

namespace ozz.wpf.Services.Interactions.Confirm;

public class ConfirmButtonType {
    public ConfirmMessageResult Button { get; set; }
    public string               Class  { get; set; }
}

public class ConfirmMessageConfig {
    public string                  Title       { get; set; }
    public string                  Message     { get; set; }
    public List<ConfirmButtonType> ButtonTypes { get; set; } = new();
}