using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ozz.wpf.Views; 

public partial class AudioRecordingsListView : UserControl {

    public AudioRecordingsListView() {
        InitializeComponent();

        //var grid = this.FindControl<DataGrid>("Recordings");
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }


}