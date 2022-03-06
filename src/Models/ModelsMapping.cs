using AutoMapper;

namespace ozz.wpf.Models;

public class ModelsMapping : Profile {

    public ModelsMapping() {

        CreateMap<AudioRecording, AudioRecording>();
    }
}