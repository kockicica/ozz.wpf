using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;

namespace ozz.wpf.ViewModels {

    public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel {

        private bool _overlayVisible;

        public MainWindowViewModel() {
            GoNext = ReactiveCommand.CreateFromObservable(() => {
                var vm = Disposition;
                return Router.Navigate.Execute(vm);
            });

            this.WhenActivated(d => {
                LoginViewModel
                    .Login
                    .Where(x => x != null)
                    .Subscribe(user => {
                        switch (user.Level) {
                            case 1:
                                Router.Navigate.Execute(Disposition);
                                break;
                        }
                    })
                    .DisposeWith(d);
            });

        }

        public DispositionViewModel Disposition { get; set; }

        public LoginViewModel LoginViewModel { get; set; }

        public bool OverlayVisible {
            get => _overlayVisible;
            set => this.RaiseAndSetIfChanged(ref _overlayVisible, value);
        }

        public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

        public ReactiveCommand<Unit, IRoutableViewModel?> GoBack => Router.NavigateBack;

        #region IActivatableViewModel Members

        public ViewModelActivator Activator { get; } = new();

        #endregion

        #region IScreen Members

        public RoutingState Router { get; } = new();

        #endregion

    }

}