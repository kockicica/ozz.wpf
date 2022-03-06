using System;

using Avalonia;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Controls;

public class CustomReactiveWindow<TViewModel> : CustomWindow, IViewFor<TViewModel> where TViewModel : class {

    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    public CustomReactiveWindow() {
        this.WhenActivated(disposables => { });
        this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);

    }

    #region IViewFor<TViewModel> Members

    public TViewModel? ViewModel {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    #endregion

    private void OnDataContextChanged(object? value) {
        if (value is TViewModel viewModel) {
            ViewModel = viewModel;
        }
        else {
            ViewModel = null;
        }
    }

    private void OnViewModelChanged(object? value) {
        if (value == null) {
            ClearValue(DataContextProperty);
        }
        else if (DataContext != value) {
            DataContext = value;
        }
    }
}