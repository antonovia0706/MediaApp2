using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaApp2.Services;
using System;
using System.Threading.Tasks;

namespace MediaApp2.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    public event Action<Models.User>? OnLoginSuccess;

    [ObservableProperty] private string _login = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public LoginViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Заполните все поля";
            return;
        }

        var user = await _dataService.AuthenticateAsync(Login, Password);
        if (user != null) OnLoginSuccess?.Invoke(user);
        else ErrorMessage = "Неверный логин или пароль";
    }
}