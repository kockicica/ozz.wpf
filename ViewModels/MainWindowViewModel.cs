using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using ReactiveUI;

using Splat;

namespace ozz.wpf.ViewModels {

    public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel {

        private bool _overlayVisible;

        public MainWindowViewModel() {

            this.WhenActivated(d => {
                LoginViewModel
                    .Login
                    .Where(x => x != null)
                    .Subscribe(user => {
                        switch (user.Level) {
                            case 1:
                                Router.Navigate.Execute(Disposition);
                                break;
                            default:
                                var vm = Locator.Current.GetService<ManagerViewModel>();
                                Router.Navigate.Execute(vm);
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

        #region IActivatableViewModel Members

        public ViewModelActivator Activator { get; } = new();

        #endregion

        #region IScreen Members

        public RoutingState Router { get; } = new();

        #endregion

    }

}