namespace ozz.wpf.Services;

public interface IResolver {
    T GetService<T>();
}