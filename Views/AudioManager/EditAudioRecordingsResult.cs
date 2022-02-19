using ozz.wpf.Dialog;
using ozz.wpf.Models;

namespace ozz.wpf.Views.AudioManager;

public class EditAudioRecordingsResult : DialogResultBase {

    public AudioRecording? Recording { get; set; }
}