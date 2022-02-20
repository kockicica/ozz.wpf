using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager;

public partial class ScheduleManagerView : ReactiveUserControl<ScheduleManagerViewModel> {
    bool _isEdit;

    DataGrid grid;

    public ScheduleManagerView() {
        InitializeComponent();

        grid = this.FindControl<DataGrid>("SchedulesGrid");

        this.WhenActivated(d => {

            // this.BindCommand(ViewModel, model => model.BeginEdit, view => view.grid, nameof(DataGrid.BeginningEdit)).DisposeWith(d);
            // this.BindCommand(ViewModel, model => model.EndEdit, view => view.grid, nameof(DataGrid.RowEditEnding)).DisposeWith(d);

            Observable.Merge(
                          Observable.FromEventPattern<DataGridRowEditEndingEventArgs>(grid, nameof(DataGrid.RowEditEnding)).Select(_ => false),
                          Observable.FromEventPattern<DataGridRowEditEndedEventArgs>(grid, nameof(DataGrid.RowEditEnded)).Select(_ => false),
                          Observable.FromEventPattern<DataGridCellEditEndingEventArgs>(grid, nameof(DataGrid.CellEditEnding)).Select(_ => false),
                          Observable.FromEventPattern<DataGridBeginningEditEventArgs>(grid, nameof(DataGrid.BeginningEdit)).Select(_ => true),
                          Observable.FromEventPattern<DataGridPreparingCellForEditEventArgs>(grid, nameof(DataGrid.PreparingCellForEdit))
                                    .Select(_ => true)
                      )
                      .Subscribe(b => { ViewModel!.IsInEdit = b; })
                      .DisposeWith(d);
            // Observable.FromEventPattern<DataGridBeginningEditEventArgs>(grid, nameof(DataGrid.BeginningEdit))
            //           .Subscribe(pattern => { ViewModel!.IsInEdit = true; })
            //           .DisposeWith(d);
            // Observable.FromEventPattern<DataGridRowEditEndingEventArgs>(grid, nameof(DataGrid.RowEditEnding))
            //           .Subscribe(pattern => { ViewModel!.IsInEdit = false; })
            //           .DisposeWith(d);
            // Observable.FromEventPattern<DataGridRowEditEndedEventArgs>(grid, nameof(DataGrid.RowEditEnded))
            //           .Subscribe(pattern => { ViewModel!.IsInEdit = false; })
            //           .DisposeWith(d);
            // Observable.FromEventPattern<DataGridCellEditEndingEventArgs>(grid, nameof(DataGrid.CellEditEnding))
            //           .Subscribe(pattern => { ViewModel!.IsInEdit = false; })
            //           .DisposeWith(d);
            // Observable.FromEventPattern<DataGridPreparingCellForEditEventArgs>(grid, nameof(DataGrid.PreparingCellForEdit))
            //           .Subscribe(pattern => { ViewModel!.IsInEdit = true; })
            //           .DisposeWith(d);

            ViewModel?.HandleEnterKey.Subscribe(unit => {
                         if (!ViewModel.IsInEdit) {
                             //ViewModel.IsInEdit = true;
                             grid.BeginEdit();
                         }
                         else {
                             //ViewModel.IsInEdit = false;
                             grid.CommitEdit();
                         }
                     })
                     .DisposeWith(d);
        });

    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}