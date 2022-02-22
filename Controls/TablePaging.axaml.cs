using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace ozz.wpf.Controls;

public class TablePaging : TemplatedControl {

    public static readonly DirectProperty<TablePaging, int> CountProperty =
        AvaloniaProperty.RegisterDirect<TablePaging, int>(nameof(Count), paging => paging.Count, (paging, i) => paging.Count = i);

    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<TablePaging, int>(nameof(CurrentPage), 1, true, defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly DirectProperty<TablePaging, int> TotalPagesProperty =
        AvaloniaProperty.RegisterDirect<TablePaging, int>(nameof(TotalPages), paging => paging.TotalPages, (paging, i) => paging.TotalPages = i);

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<TablePaging, int>(nameof(PageSize), defaultValue: 20, defaultBindingMode: BindingMode.TwoWay, coerce: Coerce);

    public static readonly DirectProperty<TablePaging, IEnumerable> PageSizesProperty =
        AvaloniaProperty.RegisterDirect<TablePaging, IEnumerable>(nameof(PageSizes),
                                                                  paging => paging.PageSizes,
                                                                  (paging, ints) => paging.PageSizes = ints);

    public static readonly RoutedEvent<RoutedEventArgs> CurrentPageChangedEvent =
        RoutedEvent.Register<TablePaging, RoutedEventArgs>(nameof(CurrentPageChanged), RoutingStrategies.Bubble);

    private int _count;

    private int _currentPage;

    private IDisposable _disposable;

    private Button? _firstPage;
    private Button? _lastPage;
    private Button? _nextPage;

    private int _pageSize;

    private IEnumerable _pageSizes;
    private Button?     _prevPage;

    private int _totalPages;

    static TablePaging() {
        FocusableProperty.OverrideDefaultValue<TablePaging>(true);
        CountProperty.Changed.AddClassHandler<TablePaging>((paging, args) => paging.HandlePropertyChanged(args));
        PageSizeProperty.Changed.AddClassHandler<TablePaging>((paging, args) => paging.HandlePropertyChanged(args));
        CurrentPageProperty.Changed.AddClassHandler<TablePaging>((paging, args) => paging.HandlePropertyChanged(args));
    }



    public TablePaging() {

        var ps = new List<int> { 10, 20, 50, 100 };
        PageSizes = ps;
        //PageSize = 20;
        //CurrentPage = 1;
        TotalPages = 0;

    }

    public int Count {
        get => _count;
        set => SetAndRaise(CountProperty, ref _count, value);
    }

    public int CurrentPage {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int PageSize {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public IEnumerable PageSizes {
        get => _pageSizes;
        set => SetAndRaise(PageSizesProperty, ref _pageSizes, value);
    }

    public int TotalPages {
        get => _totalPages;
        set => SetAndRaise(TotalPagesProperty, ref _totalPages, value);
    }

    public Button? FirstPage {
        get => _firstPage;
        set {
            if (_firstPage != null) {
                _firstPage.Click -= HandleButtonClick;
            }
            _firstPage = value;
            if (_firstPage != null) {
                _firstPage.Click += HandleButtonClick;
            }
        }
    }

    public Button? NextPage {
        get => _nextPage;
        set {
            if (_nextPage != null) {
                _nextPage.Click -= HandleButtonClick;
            }
            _nextPage = value;
            if (_nextPage != null) {
                _nextPage.Click += HandleButtonClick;
            }
        }
    }

    public Button? PrevPage {
        get => _prevPage;
        set {
            if (_prevPage != null) {
                _prevPage.Click -= HandleButtonClick;
            }
            _prevPage = value;
            if (_prevPage != null) {
                _prevPage.Click += HandleButtonClick;
            }
        }
    }

    public Button? LastPage {
        get => _lastPage;
        set {
            if (_lastPage != null) {
                _lastPage.Click -= HandleButtonClick;
            }
            _lastPage = value;
            if (_lastPage != null) {
                _lastPage.Click += HandleButtonClick;
            }
        }
    }

    public event EventHandler<RoutedEventArgs> CurrentPageChanged {
        add => AddHandler(CurrentPageChangedEvent, value);
        remove => RemoveHandler(CurrentPageChangedEvent, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change) {
        base.OnPropertyChanged(change);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        FirstPage = e.NameScope.Find<Button>("PART_FirstPage");
        NextPage = e.NameScope.Find<Button>("PART_NextPage");
        PrevPage = e.NameScope.Find<Button>("PART_PrevPage");
        LastPage = e.NameScope.Find<Button>("PART_LastPage");
        SetupButtons();

    }

    private static int Coerce(IAvaloniaObject o, int value) {
        return o.GetValue(PageSizesProperty).Cast<int>().Contains(value) ? value : 20;
    }

    private void HandlePropertyChanged(AvaloniaPropertyChangedEventArgs args) {
        var a = args;
        TotalPages = GetTotalPages();
        SetupButtons();
        if (args.Property.Name == nameof(CurrentPageProperty)) {
            var eventArgs = new RoutedEventArgs(CurrentPageChangedEvent);
            RaiseEvent(eventArgs);
        }
    }

    private int GetTotalPages() {
        if (PageSize == 0) {
            return 0;
        }
        var (quotient, remainder) = Math.DivRem(Count, PageSize);
        return remainder > 0 ? quotient + 1 : quotient;
    }

    private void SetupButtons() {
        if (FirstPage != null) {
            FirstPage.IsEnabled = CurrentPage != 1;
            //_firstPage.AddDisposableHandler(Button.ClickEvent, HandleButtonClick);
        }
        if (PrevPage != null) {
            PrevPage.IsEnabled = CurrentPage > 1;
            //_prevPage.AddDisposableHandler(Button.ClickEvent, HandleButtonClick);

        }
        if (NextPage != null) {
            NextPage.IsEnabled = CurrentPage < TotalPages && TotalPages != 0;
            //_nextPage.AddDisposableHandler(Button.ClickEvent, HandleButtonClick);
        }
        if (LastPage != null) {
            LastPage.IsEnabled = CurrentPage != TotalPages && TotalPages != 0;
            //_lastPage.AddDisposableHandler(Button.ClickEvent, HandleButtonClick);
        }
    }

    private void HandleButtonClick(object? sender, RoutedEventArgs e) {
        if (!e.Handled) {
            if (sender is Button btn) {
                switch (btn.Name) {
                    case "PART_FirstPage":
                        CurrentPage = 1;
                        break;
                    case "PART_NextPage":
                        CurrentPage += 1;
                        break;
                    case "PART_PrevPage":
                        CurrentPage -= 1;
                        break;
                    case "PART_LastPage":
                        CurrentPage = TotalPages;
                        break;

                }
                e.Handled = true;
            }
        }
    }
}