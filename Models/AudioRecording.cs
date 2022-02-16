using System;

namespace ozz.wpf.Models;

public class AudioRecording : HasId {
    public string   Name     { get; set; }
    public string   Path     { get; set; }
    public string   Category { get; set; }
    public long     Duration { get; set; }
    public DateTime Date     { get; set; }
    public bool     Active   { get; set; }

    #region HasId Members

    public int Id { get; set; }

    #endregion

}