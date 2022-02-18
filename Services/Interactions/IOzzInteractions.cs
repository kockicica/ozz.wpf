using System.Reactive;

using ozz.wpf.Models;
using ozz.wpf.Services.Interactions.Confirm;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public interface IOzzInteractions {

    Interaction<BrowseForFileConfig, string>                Browse               { get; }
    Interaction<AudioRecording, AudioRecording?>            EditAudioRecording   { get; }
    Interaction<Unit, AudioRecording?>                      CreateAudioRecording { get; }
    Interaction<ConfirmMessageConfig, ConfirmMessageResult> Confirm              { get; }
}