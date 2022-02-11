using System;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Utilities;

using JetBrains.Annotations;

namespace ozz.wpf.Controls;

public class ValueChangedEventArgs : RoutedEventArgs {

    public ValueChangedEventArgs() { }
    public ValueChangedEventArgs([CanBeNull] RoutedEvent? routedEvent) : base(routedEvent) { }
    public ValueChangedEventArgs([CanBeNull] RoutedEvent? routedEvent, [CanBeNull] IInteractive? source) : base(routedEvent, source) { }
    public double Value { get; set; }

}

[PseudoClasses(":vertical", ":horizontal", ":pressed")]
public class PlayerSlider : RangeBase {

    private Track?       _track;
    private Button?      _decreaseButton;
    private Button?      _increaseButton;
    private IDisposable? _decreaseButtonPressDispose;
    private IDisposable? _decreaseButtonReleaseDispose;
    private IDisposable? _increaseButtonSubscription;
    private IDisposable? _increaseButtonReleaseDispose;
    private IDisposable? _pointerMovedDispose;
    private bool         _isDragging = false;

    private const double Tolerance = 0.0001;

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<Slider>();

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        Track.IsDirectionReversedProperty.AddOwner<Slider>();

    public static readonly DirectProperty<PlayerSlider, double> OutsideValueProperty =
        AvaloniaProperty.RegisterDirect<PlayerSlider, double>(
            nameof(OutsideValue),
            o => o.Value,
            (slider, d) => {
                if (!slider._isDragging) {
                    slider.Value = d;
                }
            },
            defaultBindingMode: BindingMode.TwoWay
        );

    public static readonly RoutedEvent<ValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<PlayerSlider, ValueChangedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    static PlayerSlider() {
        PressedMixin.Attach<PlayerSlider>();
        FocusableProperty.OverrideDefaultValue<PlayerSlider>(true);
        Thumb.DragStartedEvent.AddClassHandler<PlayerSlider>((slider, args) => slider.OnThumbDragStarted(args), RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<PlayerSlider>((slider, args) => slider.OnThumbDragCompleted(args), RoutingStrategies.Bubble);
        ValueProperty.OverrideMetadata<PlayerSlider>(new DirectPropertyMetadata<double>(enableDataValidation: true));
    }

    public PlayerSlider() {
        UpdatePseudoClasses(Orientation);
    }

    public Orientation Orientation {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsDirectionReversed {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    public double OutsideValue {
        get => Value;
        set => Value = value;
    }

    public event EventHandler<ValueChangedEventArgs>? ValueChanged {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        _decreaseButtonPressDispose?.Dispose();
        _decreaseButtonReleaseDispose?.Dispose();
        _increaseButtonSubscription?.Dispose();
        _increaseButtonReleaseDispose?.Dispose();
        _pointerMovedDispose?.Dispose();

        _decreaseButton = e.NameScope.Find<Button>("PART_DecreaseButton");
        _track = e.NameScope.Find<Track>("PART_Track");
        _increaseButton = e.NameScope.Find<Button>("PART_IncreaseButton");

        if (_track != null) {
            _track.IsThumbDragHandled = true;
        }

        if (_decreaseButton != null) {
            _decreaseButtonPressDispose = _decreaseButton.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _decreaseButtonReleaseDispose = _decreaseButton.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        }

        if (_increaseButton != null) {
            _increaseButtonSubscription = _increaseButton.AddDisposableHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _increaseButtonReleaseDispose = _increaseButton.AddDisposableHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        }

        _pointerMovedDispose = this.AddDisposableHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);


    }

    protected virtual void OnThumbDragStarted(VectorEventArgs args) {
        _isDragging = true;
    }

    protected virtual void OnThumbDragCompleted(VectorEventArgs args) {
        OnValueChanged();
        _isDragging = false;
    }

    protected virtual void OnValueChanged() {
        if (IsEffectivelyEnabled) {

            var e = new ValueChangedEventArgs(ValueChangedEvent) { Value = Value };
            RaiseEvent(e);

            // if (!e.Handled && Command?.CanExecute(CommandParameter) == true)
            // {
            //     Command.Execute(CommandParameter);
            //     e.Handled = true;
            // }
        }
    }


    private void TrackMoved(object? sender, PointerEventArgs e) {
        if (_isDragging) {
            MoveToPoint(e.GetCurrentPoint(_track));
        }
    }

    private void TrackPressed(object? sender, PointerPressedEventArgs e) {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            MoveToPoint(e.GetCurrentPoint(_track));
            _isDragging = true;
        }
    }

    private void TrackReleased(object? sender, PointerReleasedEventArgs e) {
        _isDragging = false;
        OnValueChanged();
    }

    private void MoveToPoint(PointerPoint posOnTrack) {
        if (_track == null) {
            return;
        }

        // var or = Orientation == Orientation.Horizontal;
        // var thumbLength = (or ? _track.Thumb.Bounds.Width : _track.Thumb.Bounds.Height) + double.Epsilon;
        // var trackLength = (or ? _track.Bounds.Width : _track.Bounds.Height) - thumbLength;
        // var trackPos = or ? posOnTrack.Position.X : posOnTrack.Position.Y;
        //
        // var logicalPos = MathUtilities.Clamp((trackPos - thumbLength * 0.5) / trackLength, 0.0d, 1.0d);
        // var invert = 0;
        // var calcVal = Math.Abs(invert - logicalPos);
        // var range = Maximum - Minimum;
        // var finalValue = calcVal * range + Minimum;

        var finalValue = GetValueFromPositionOnTrack(posOnTrack);
        Value = finalValue;

    }

    private double GetValueFromPositionOnTrack(PointerPoint posOnTrack) {

        var or = Orientation == Orientation.Horizontal;
        var thumbLength = (or ? _track.Thumb.Bounds.Width : _track.Thumb.Bounds.Height) + double.Epsilon;
        var trackLength = (or ? _track.Bounds.Width : _track.Bounds.Height) - thumbLength;
        var trackPos = or ? posOnTrack.Position.X : posOnTrack.Position.Y;

        var logicalPos = MathUtilities.Clamp((trackPos - thumbLength * 0.5) / trackLength, 0.0d, 1.0d);
        var invert = 0;
        var calcVal = Math.Abs(invert - logicalPos);
        var range = Maximum - Minimum;
        var finalValue = calcVal * range + Minimum;

        return finalValue;


    }

    private void UpdatePseudoClasses(Orientation o) {
        PseudoClasses.Set(":vertical", o == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
    }


}