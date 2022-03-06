using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public partial class CreateScheduleView : ReactiveUserControl<CreateScheduleViewModel> {

    private Button              _backButton;
    private Button              _cancelButton;
    private IRoutableViewModel? _currentViewModel;
    private Button              _finishButton;
    private Button              _nextButton;

    private RoutedViewHost _viewHost;

    public CreateScheduleView() {
        InitializeComponent();

        _backButton = this.FindControl<Button>("BackButton");
        _nextButton = this.FindControl<Button>("NextButton");
        _finishButton = this.FindControl<Button>("FinishButton");
        _cancelButton = this.FindControl<Button>("CancelButton");
        _viewHost = this.FindControl<RoutedViewHost>("ViewHost");

        //SetupButtons();



    }


    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private bool BackEnabled() {
        return true;
    }

    private void SetupButtons() {
        if (_currentViewModel == null) {
            _backButton.IsVisible = _cancelButton.IsVisible = _finishButton.IsVisible = false;
            _nextButton.IsVisible = true;
        }
    }
}