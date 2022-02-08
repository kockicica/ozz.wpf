using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using JetBrains.Annotations;

using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Dialog;

public class DialogViewModelBase<TResult> : ViewModelBase where TResult : DialogResultBase {

    //public event EventHandler<DialogResultEventArgs<TResult>> CloseRequested;

    protected readonly Subject<TResult> _closeRequested = new();

    public ICommand CloseCommand { get; }

    protected DialogViewModelBase() {
        CloseCommand = ReactiveCommand.Create(Close);
    }

    protected void Close() => Close(default);

    protected void Close(TResult result) {
        _closeRequested.OnNext(result);
        _closeRequested.OnCompleted();
    }

    public IObservable<TResult> CloseRequested => _closeRequested.AsObservable();

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

public class DialogViewModelBase : DialogViewModelBase<DialogResultBase> { }