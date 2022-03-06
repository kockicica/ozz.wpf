using System.Collections.Generic;
using System.Windows.Input;

using ozz.wpf.Dialog;
using ozz.wpf.Services.Interactions.Confirm;

using ReactiveUI;

namespace ozz.wpf.Views.Dialogs;

public class ConfirmDialogResult : DialogResultBase {
    public ConfirmMessageResult Result { get; set; }
}

public class ConfirmDialogViewModel : DialogViewModelBase<ConfirmDialogResult> {

    private List<ConfirmButtonType> _buttonTypes = new();

    private string _message;

    public ConfirmDialogViewModel() {
        CommandYes = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.Yes }));
        CommandNo = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.No }));
    }

    public string Message {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public ICommand CommandYes { get; }
    public ICommand CommandNo  { get; }

    public List<ConfirmButtonType> ButtonTypes {
        get => _buttonTypes;
        set => this.RaiseAndSetIfChanged(ref _buttonTypes, value);
    }

    // public string[] YesButtonClasses => new string[] { ButtonTypes.SingleOrDefault(type => type.Button == ConfirmMessageResult.Yes)?.Class };
    // public string[] NoButtonClasses  => new string[] { ButtonTypes.SingleOrDefault(type => type.Button == ConfirmMessageResult.No)?.Class };
}