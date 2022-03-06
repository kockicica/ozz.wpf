using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Dialog;

public class DialogViewModelBase<TResult> : ViewModelBase, IActivatableViewModel where TResult : DialogResultBase {

    //public event EventHandler<DialogResultEventArgs<TResult>> CloseRequested;

    protected readonly Subject<TResult> _closeRequested = new();

    protected DialogViewModelBase() {
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public ICommand CloseCommand { get; }

    public IObservable<TResult> CloseRequested => _closeRequested.AsObservable();

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    protected void Close() => Close(default);

    protected void Close(TResult result) {
        _closeRequested.OnNext(result);
        _closeRequested.OnCompleted();
    }
}

// public static class EventExtensions {
//
//     public static void Raise<TEventArgs>(
//         this EventHandler<TEventArgs>? eventHandler,
//         object sender,
//         TEventArgs args) where TEventArgs : EventArgs {
//         var handler = Volatile.Read(ref eventHandler);
//
//         handler?.Invoke(sender, args);
//     }
//
// }

public class DialogViewModelBase : DialogViewModelBase<DialogResultBase> {
}