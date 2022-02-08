using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Dialog;

public class DialogWindowBase<TResult> : ReactiveWindow<DialogViewModelBase<TResult>>
    where TResult : DialogResultBase {

    private Window ParentWindow => (Window)Owner;

    protected DialogWindowBase() {
        
        this.WhenActivated(disposables => {

            ViewModel!.CloseRequested.Subscribe(Observer.Create<TResult>(Close)).DisposeWith(disposables);

            Observable.FromEventPattern<EventArgs>(this, "Opened").Do(_ => {
                CenterDialog();
                LockSize();
            }).Subscribe().DisposeWith(disposables);

        });
    }
    
    private void CenterDialog()
    {
        var x = ParentWindow.Position.X + (ParentWindow.Bounds.Width - Width) / 2;
        var y = ParentWindow.Position.Y + (ParentWindow.Bounds.Height - Height) / 2;

        Position = new PixelPoint((int) x, (int) y);
    }

    private void LockSize()
    {
        MaxWidth = MinWidth = Width;
        MaxHeight = MinHeight = Height;
    }    


}

public class DialogWindowBase : DialogWindowBase<DialogResultBase> { }