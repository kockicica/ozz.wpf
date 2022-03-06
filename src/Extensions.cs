using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ozz.wpf;

public static class Extensions {


    public static IEnumerable<TResult?> AddNull<TResult>(this IEnumerable<TResult?> source) {
        var p = new List<TResult> { default };
        p.AddRange(source);
        return p.AsEnumerable();
    }

    public static IObservable<IEnumerable<TResult?>> AddNull<TResult>(this IObservable<IEnumerable<TResult?>> source) {
        return source.Select(AddNull);
    }
}