using System.Collections.Generic;

using ozz.wpf.Models;

namespace ozz.wpf.Services;

public static class Extensions {

    public static string ToQueryString(this ScheduleSearchParams sp) {
        var items = new List<string>();
        if (sp.Recording.HasValue) {
            items.Add($"recording={sp.Recording}");
        }
        if (sp.FromDate.HasValue) {
            items.Add($"fromDate={sp.FromDate:yyyy-MM-dd}");
        }
        if (sp.ToDate.HasValue) {
            items.Add($"toDate={sp.ToDate:yyyy-MM-dd}");
        }

        return $"?{string.Join("&", items)}";
    }

    public static string ToQueryString(this DispositionSearchParams sp) {
        var items = new List<string>();
        items.Add($"date={sp.Date:yyyy-MM-dd}");
        items.Add($"shift={sp.Shift}");
        return $"?{string.Join("&", items)}";
    }
}