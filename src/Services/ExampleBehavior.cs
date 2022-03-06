using System;
using System.Collections;

using Avalonia;
using Avalonia.Controls;

namespace ozz.wpf.Services;

public class ExampleBehavior : AvaloniaObject {


    public static readonly AttachedProperty<IEnumerable> ItemsProperty = AvaloniaProperty.RegisterAttached<ExampleBehavior, ComboBox, IEnumerable>(
        "Items");

    static ExampleBehavior() {
        ItemsProperty.Changed.Subscribe(HandleItemsPropertyChanged);
    }

    public static void SetItems(AvaloniaObject element, IEnumerable value) {
        element.SetValue(ItemsProperty, value);
    }

    public static IEnumerable GetItems(AvaloniaObject element) {
        return element.GetValue(ItemsProperty);
    }

    private static void HandleItemsPropertyChanged(AvaloniaPropertyChangedEventArgs<IEnumerable> obj) {
        var a = 10;
    }
}