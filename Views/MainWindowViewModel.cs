using System;
using System.Reactive;
using System.Reactive.Disposables;

using Avalonia.Controls.Notifications;

using ozz.wpf.ViewModels;
using ozz.wpf.Views.Manager;

using ReactiveUI;

using Splat;

using Notification = Avalonia.Controls.Notifications.Notification;

namespace ozz.wpf.Views {

    public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel {

        private bool _overlayVisible;

        public MainWindowViewModel() {

            Close = ReactiveCommand.Create(() => { });

            this.WhenActivated(d => {
                LoginViewModel
                    .Login
                    //.Where(x => x != null)
                    .Subscribe(user => {
                        if (user != null) {
                            switch (user.Level) {
                                case 1:
                                    Router.Navigate.Execute(Disposition);
                                    break;
                                default:
                                    var vm = Locator.Current.GetService<ManagerViewModel>();
                                    Router.Navigate.Execute(vm);
                                    break;
                            }
                        }
                        else {
                            this.NotificationManager.Show(
                                new Notification("Greška", "Korisnik ne postoji", NotificationType.Error));
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

        public INotificationManager NotificationManager { get; set; }

        public ReactiveCommand<Unit, Unit> Close { get; }

        #region IActivatableViewModel Members

        public ViewModelActivator Activator { get; } = new();

        #endregion

        #region IScreen Members

        public RoutingState Router { get; } = new();

        #endregion

    }

}