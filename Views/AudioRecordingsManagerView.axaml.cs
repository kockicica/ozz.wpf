using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class AudioRecordingsManagerView : ReactiveUserControl<AudioRecordingsManagerViewModel> {

    private ComboBox cb;
    private DataGrid grid;

    public AudioRecordingsManagerView() {
        InitializeComponent();

        cb = this.FindControl<ComboBox>("CategoriesCombo");
        grid = this.FindControl<DataGrid>("Grid");


        this.WhenActivated(d => {
            Observable.FromEventPattern<DataGridColumnEventArgs>(grid, "Sorting")
                      .Subscribe(pattern => {
                          var sort = pattern.EventArgs.Column.SortMemberPath;
                      })
                      .DisposeWith(d);
        });



    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}