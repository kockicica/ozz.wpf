using System;

using Splat;

namespace ozz.wpf.Services;

public class LocatorBasedResolver : IResolver {

    #region IResolver Members

    public T GetService<T>() {
        var fnd = Locator.Current.GetService<T>();
        if (fnd == null) {
            throw new ApplicationException($"Unable to resolve service:{typeof(T)}");
        }
        return fnd;
    }

    #endregion

}