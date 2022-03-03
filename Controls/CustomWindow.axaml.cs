using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
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

    ContentPresenter _contentPresenter;

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

    IAvaloniaReadOnlyList<string> IStyleable.Classes => new Classes("ModalWindow");

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
        _contentPresenter.PropertyChanged += (sender, args) => {
            if (args.Property == ContentProperty) {
                ((IStyledElement)this).InvalidateStyles();
            }
        };

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
}