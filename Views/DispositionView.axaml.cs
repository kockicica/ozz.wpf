using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ozz.wpf.Views; 

public partial class DispositionView : UserControl {

    public DispositionView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

}