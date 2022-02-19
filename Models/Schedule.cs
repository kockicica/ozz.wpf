using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ozz.wpf.Models;

public class CreateScheduleData {
    public int      Recording      { get; set; }
    public DateTime Date           { get; set; }
    public int      Shift1         { get; set; }
    public int      Shift2         { get; set; }
    public int      Shift3         { get; set; }
    public int      Shift4         { get; set; }
    public int      TotalPlayCount { get; set; }
}

public class Schedule : HasId {
    public AudioRecording Recording { get; set; }
    public DateTime       Date      { get; set; }

    [JsonConverter(typeof(TimeSpanDurationJsonConverter))]
    public TimeSpan Duration { get; set; }

    public int Shift1         { get; set; }
    public int Shift2         { get; set; }
    public int Shift3         { get; set; }
    public int Shift4         { get; set; }
    public int Shift1Played   { get; set; }
    public int Shift2Played   { get; set; }
    public int Shift3Played   { get; set; }
    public int Shift4Played   { get; set; }
    public int TotalPlayCount { get; set; }

    public IEnumerable<Disposition> Dispositions { get; set; }

    #region HasId Members

    public int Id { get; set; }

    #endregion

}

public class Disposition : AudioRecording {
    public int Shift            { get; set; }
    public int PlayCountNeeded  { get; set; }
    public int PlayCountCurrent { get; set; }
    public int RecordingId      { get; set; }
}

public class ScheduleSearchParams {
    public int?      Recording { get; set; }
    public DateTime? FromDate  { get; set; }
    public DateTime? ToDate    { get; set; }
}

public class TimeSpanDurationJsonConverter : JsonConverter<TimeSpan> {
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (typeToConvert == typeof(long)) {
            if (!reader.TryGetInt64(out var duration)) {
                return TimeSpan.Zero;
            }
            return TimeSpan.FromMilliseconds(duration / 1_000_000);
        }
        return TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) {
        writer.WriteNumberValue(value.TotalMilliseconds * 1_000_000);
    }
}