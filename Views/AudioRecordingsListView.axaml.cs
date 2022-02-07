using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ozz.wpf.Views; 

public partial class AudioRecordingsListView : UserControl {

    public AudioRecordingsListView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

}