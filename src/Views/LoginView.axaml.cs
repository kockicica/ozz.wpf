using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel> {

    private TextBox _usernameBox;

    public LoginView() {
        InitializeComponent();

        _usernameBox = this.FindControl<TextBox>("UsernameBlock");

        this.WhenActivated(d => {
            // Observable.FromEventPattern<VisualTreeAttachmentEventArgs>(_usernameBox, "AttachedToVisualTree")
            //           .Subscribe(pattern => _usernameBox.Focus())
            //           .DisposeWith(d);
        });
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}