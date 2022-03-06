using System.Collections.Generic;
using System.Reactive;

using ozz.wpf.Models;
using ozz.wpf.Services.Interactions.Confirm;
using ozz.wpf.Views.Disposition;

using ReactiveUI;

namespace ozz.wpf.Services.Interactions;

public interface IOzzInteractions {

    Interaction<BrowseForFileConfig, string>                Browse               { get; }
    Interaction<AudioRecording, AudioRecording?>            EditAudioRecording   { get; }
    Interaction<Unit, AudioRecording?>                      CreateAudioRecording { get; }
    Interaction<ConfirmMessageConfig, ConfirmMessageResult> Confirm              { get; }
    Interaction<AudioRecording, Unit>                       ShowPlayer           { get; }
    Interaction<Unit, IEnumerable<Schedule>>                CreateSchedules      { get; }
    Interaction<Unit, Unit>                                 CreateDispositions   { get; }
    Interaction<Unit, DispositionSelectItem>                SelectDisposition    { get; }
    Interaction<DispositionBlock, Unit>                     ShowBlockPlayer      { get; }
}