using ozz.wpf.Dialog;
using ozz.wpf.Models;

namespace ozz.wpf.ViewModels;

public class EditAudioRecordingsResult : DialogResultBase {

    public AudioRecording? Recording { get; set; }
}