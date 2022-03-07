using System.Collections.Generic;
using System.Linq;
using System.Web;

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

    public static string ToQueryString(this AudioRecordingsSearchParams sp) {
        var items = new List<string>();
        if (sp.Active.HasValue) {
            items.Add($"active={sp.Active.Value}");
        }
        if (!string.IsNullOrEmpty(sp.Name)) {
            items.Add($"name={HttpUtility.UrlEncode(sp.Name)}");
        }
        if (sp.CategoryId.HasValue) {
            items.Add($"category={sp.CategoryId}");
        }
        if (sp.FromDate.HasValue) {
            items.Add($"fromDate={sp.FromDate.Value:yyyy-MM-dd}");
        }
        if (sp.ToDate.HasValue) {
            items.Add($"toDate={sp.ToDate.Value:yyyy-MM-dd}");
        }
        if (sp.Count.HasValue) {
            items.Add($"count={sp.Count}");
        }
        if (sp.Skip.HasValue) {
            items.Add($"skip={sp.Skip}");
        }
        if (!string.IsNullOrEmpty(sp.Sort)) {
            items.Add($"sort={HttpUtility.UrlEncode(sp.Sort)}");
        }
        return items.Any() ? $"?{string.Join("&", items)}" : string.Empty;

    }
}