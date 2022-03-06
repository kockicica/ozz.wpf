using System;

namespace ozz.wpf.Dialog;

public class DialogResultEventArgs<TResult> : EventArgs {

    public DialogResultEventArgs(TResult result) {
        Result = result;
    }

    public TResult Result { get; }
}