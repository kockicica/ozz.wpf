using ReactiveUI;

namespace ozz.wpf.ViewModels {

    public class MainWindowViewModel : ViewModelBase {

        public DispositionViewModel Disposition { get; set; }

        public string Greeting => "Welcome to Avalonia!";

        public bool OverlayVisible {
            get => _overlayVisible;
            set => this.RaiseAndSetIfChanged(ref _overlayVisible, value);
        }

        // public MainWindowViewModel(DispositionViewModel disposition) {
        //     Disposition = disposition;
        // }

        private bool _overlayVisible;
        

    }

}