namespace ozz.wpf.Views.ScheduleManager.CreateSchedule;

public interface IWizardPage {
    bool IsValid();
    bool IsFinal();
}

public interface IWizard {

    void Next();
}