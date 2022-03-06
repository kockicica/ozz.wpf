using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace ozz.wpf.Behaviors;

public class OnKeyDownTappedTriggerBehavior : Trigger<Control> {

    public static readonly StyledProperty<Control?> TargetControlProperty =
        AvaloniaProperty.Register<OnKeyDownTappedTriggerBehavior, Control?>(nameof(TargetControl));

    public static readonly StyledProperty<Key> KeyProperty = AvaloniaProperty.Register<OnKeyDownTappedTriggerBehavior, Key>(nameof(Key));

    public Control? TargetControl {
        get => GetValue(TargetControlProperty);
        set => SetValue(TargetControlProperty, value);
    }

    public Key Key {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    protected override void OnAttached() {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, HandleKeyDown);
    }

    protected override void OnDetaching() {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, HandleKeyDown);
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key) {
            Interaction.ExecuteActions(sender, Actions, e);
            e.Handled = true;
        }
    }
}