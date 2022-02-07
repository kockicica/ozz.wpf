using System;

namespace ozz.wpf.Models;

public class AudioRecording {

    public int      Id       { get; set; }
    public string   Name     { get; set; }
    public string   Path     { get; set; }
    public string   Category { get; set; }
    public long     Duration { get; set; }
    public DateTime Date     { get; set; }

}