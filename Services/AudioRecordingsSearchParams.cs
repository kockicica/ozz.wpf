using System;

namespace ozz.wpf.Services;

public class AudioRecordingsSearchParams {
    public int?      CategoryId { get; set; }
    public string?   Name       { get; set; }
    public DateTime? FromDate   { get; set; }
    public DateTime? ToDate     { get; set; }
    public bool?     Active     { get; set; }
}