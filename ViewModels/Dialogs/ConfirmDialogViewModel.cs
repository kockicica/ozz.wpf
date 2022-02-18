using System.Windows.Input;

using ozz.wpf.Dialog;
using ozz.wpf.Services.Interactions.Confirm;

using ReactiveUI;

namespace ozz.wpf.ViewModels.Dialogs;

public class ConfirmDialogResult : DialogResultBase {
    public ConfirmMessageResult Result { get; set; }
}

public class ConfirmDialogViewModel : DialogViewModelBase<ConfirmDialogResult> {

    private string _message;

    public ConfirmDialogViewModel() {
        // CommandYes = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.Yes }), Observable.Return(true));
        // CommandNo = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.No }), Observable.Return(true));
        CommandYes = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.Yes }));
        CommandNo = ReactiveCommand.Create(() => Close(new ConfirmDialogResult { Result = ConfirmMessageResult.No }));
    }

    public string Message {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public ICommand CommandYes { get; }
    public ICommand CommandNo  { get; }
}