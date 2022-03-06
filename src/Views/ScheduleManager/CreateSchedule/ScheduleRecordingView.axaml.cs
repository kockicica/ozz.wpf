using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class ScheduleRecordingView : ReactiveUserControl<ScheduleRecordingViewModel> {

    bool     _isEdit;
    DataGrid grid;

    public ScheduleRecordingView() {
        InitializeComponent();

        grid = this.FindControl<DataGrid>("SchedulesGrid");

        this.WhenActivated(d => {


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

            ViewModel?.HandleEnterKey.Subscribe(unit => {
                         if (!ViewModel.IsInEdit) {
                             grid.BeginEdit();
                         }
                         else {
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