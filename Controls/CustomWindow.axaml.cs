using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;

namespace ozz.wpf.Controls;

public class CustomWindow : Window, IStyleable {

    public static readonly DirectProperty<CustomWindow, bool> CanMaximizeProperty =
        AvaloniaProperty.RegisterDirect<CustomWindow, bool>(
            nameof(CanMaximize),
            o => o.CanMaximize);

    public static readonly DirectProperty<CustomWindow, bool> CanRestoreProperty =
        AvaloniaProperty.RegisterDirect<CustomWindow, bool>(
            nameof(CanRestore),
            o => o.CanRestore);

    public static readonly StyledProperty<ControlTemplate> HeaderTemplateProperty =
        AvaloniaProperty.Register<CustomWindow, ControlTemplate>
        (
            "HeaderTemplate"
        );

    public static readonly StyledProperty<object> TitleAreaContentProperty =
        AvaloniaProperty.Register<CustomWindow, object>
        (
            nameof(TitleAreaContent)
        );

    public static readonly StyledProperty<DataTemplate> TitleAreaContentTemplateProperty =
        AvaloniaProperty.Register<CustomWindow, DataTemplate>
        (
            nameof(TitleAreaContentTemplate)
        );

    public static readonly StyledProperty<ControlTemplate> ButtonsAreaTemplateProperty =
        AvaloniaProperty.Register<CustomWindow, ControlTemplate>
        (
            nameof(ButtonsAreaTemplate)
        );

    Button _closeButton;

    // public string Classes {
    //     get { return GetValue(ClassesProperty); }
    //     set {
    //         SetValue(ClassesProperty, value);
    //         
    //     }
    // }
    //
    // public static readonly StyledProperty<string> ClassesProperty =
    //     AvaloniaProperty.Register<CustomWindow, string>
    //     (
    //         nameof(Classes)
    //     );

    public CustomWindow() {

        //InitializeComponent();

// #if DEBUG
//         this.AttachDevTools();
// #endif

        (this as INotifyPropertyChanged).PropertyChanged += CustomWindow_PropertyChanged;

    }

    public bool CanMaximize => this.WindowState != WindowState.Maximized;

    public bool CanRestore =>
        this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen;


    public ControlTemplate HeaderTemplate {
        get { return GetValue(HeaderTemplateProperty); }
        set { SetValue(HeaderTemplateProperty, value); }
    }


    public object TitleAreaContent {
        get { return GetValue(TitleAreaContentProperty); }
        set { SetValue(TitleAreaContentProperty, value); }
    }

    public DataTemplate TitleAreaContentTemplate {
        get { return GetValue(TitleAreaContentTemplateProperty); }
        set { SetValue(TitleAreaContentTemplateProperty, value); }
    }

    public ControlTemplate ButtonsAreaTemplate {
        get { return GetValue(ButtonsAreaTemplateProperty); }
        set { SetValue(ButtonsAreaTemplateProperty, value); }
    }

    #region IStyleable Members

    Type IStyleable.StyleKey => typeof(CustomWindow);

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _closeButton.Click += (sender, args) => { Close(); };

    }

    private void CustomWindow_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(WindowState)) {
            RaisePropertyChanged(CanMaximizeProperty, !CanMaximize, CanMaximize);
            RaisePropertyChanged(CanRestoreProperty, !CanRestore, CanRestore);
        }

    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    //IAvaloniaReadOnlyList<string> IStyleable.Classes => new Classes("ModalWindow");
}