namespace ozz.wpf.ViewModels {

    public class MainWindowViewModel : ViewModelBase {

        public DispositionViewModel Disposition { get; set; }

        public string Greeting => "Welcome to Avalonia!";

    }

}