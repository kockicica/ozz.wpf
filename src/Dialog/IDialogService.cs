using System.Threading.Tasks;

namespace ozz.wpf.Dialog;

public interface IDialogService {

    Task<TResult> ShowDialogAsync<TResult>(string viewModelName) where TResult : DialogResultBase;

    Task ShowDialogAsync(string viewModelName);

    Task ShowDialogAsync<TParam>(string viewModelName, TParam param);
}