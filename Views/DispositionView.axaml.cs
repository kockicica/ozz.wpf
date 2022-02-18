using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using ozz.wpf.ViewModels;

using ReactiveUI;

namespace ozz.wpf.Views;

public partial class DispositionView : ReactiveUserControl<DispositionViewModel> {

    private IDisposable d;

    AudioRecordingsListView? recordings;

    public DispositionView() {
        InitializeComponent();

        recordings = this.FindControl<AudioRecordingsListView>("RecordingsListView");


        this.WhenActivated(d => {
            //recordings.Focus();
            //recordings.RecordingGrid.Focus();
        });

    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        //recordings.Focus();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }
}