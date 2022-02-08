using ReactiveUI;

namespace ozz.wpf.ViewModels {

    public class MainWindowViewModel : ViewModelBase {

        public DispositionViewModel Disposition { get; set; }

        public string Greeting => "Welcome to Avalonia!";

        public bool ShowOverlay {
            get => _showOverlay;
            set => this.RaiseAndSetIfChanged(ref _showOverlay, value);
        }

        // public MainWindowViewModel(DispositionViewModel disposition) {
        //     Disposition = disposition;
        // }

        private bool _showOverlay;
        

    }

}