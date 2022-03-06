using System;

using Avalonia.Markup.Xaml;

namespace ozz.wpf.Behaviors;

public class ContainerExtension : MarkupExtension {

    private readonly Type _type;

    public ContainerExtension(Type type) {
        _type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        //if(Design.IsDesignMode)


        try {
            return serviceProvider.GetService(_type)!;

        }
        catch (Exception) {
            return null;
        }
    }
}