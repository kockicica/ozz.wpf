using System.Collections.Generic;

namespace ozz.wpf.Models;

public class PagedResults<T> {
    public int            Count { get; set; }
    public IEnumerable<T> Data  { get; set; }
}