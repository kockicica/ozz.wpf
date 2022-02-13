using System;

using Microsoft.Extensions.Logging;

using ozz.wpf.Models;
using ozz.wpf.Services;

using ReactiveUI;

namespace ozz.wpf.ViewModels;

public class LoginViewModel : ViewModelBase, IActivatableViewModel, IRoutableViewModel {
    private readonly IClient _client;

    private readonly ILogger<LoginViewModel> _logger;

    private string _username;

    public LoginViewModel(ILogger<LoginViewModel> logger, IScreen hostScreen, IClient client) {
        _logger = logger;
        HostScreen = hostScreen;
        _client = client;

        Login = ReactiveCommand.CreateFromTask<string, User?>(async s => {
                                                                  var usr = await _client.Authorize(s);
                                                                  return usr;
                                                              },
                                                              this.WhenAnyValue<LoginViewModel, bool, string>(model => model.Username,
                                                                  s => !string.IsNullOrWhiteSpace(s)));
    }

    public string Username {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public ReactiveCommand<string, User?> Login { get; }

    #region IActivatableViewModel Members

    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region IRoutableViewModel Members

    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public IScreen HostScreen     { get; }

    #endregion

}