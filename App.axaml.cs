using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MediaApp2.Services;
using MediaApp2.Views;
using MediaApp2.ViewModels;

namespace MediaApp2;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Создаём сервисы
            var vkService = new VkService(groupId: "-123456789", accessToken: "");
            var dataService = new JsonDataService();

            // Создаём главное окно с новостями
            var mainVm = new MainViewModel(vkService, dataService);
            var mainView = new MainView { DataContext = mainVm };

            // Обработка запроса на вход
            mainVm.OnLoginRequested += () =>
            {
                var loginVm = new LoginViewModel(dataService);
                var loginView = new LoginView { DataContext = loginVm };

                loginVm.OnLoginSuccess += (user) =>
                {
                    loginView.Close();
                    mainVm.CurrentUser = user;
                    mainVm.IsLoggedIn = true;
                    mainVm.IsAdmin = user.Role == "admin";
                    mainVm.StatusMessage = $"Добро пожаловать, {user.Login}!";
                };

                loginView.ShowDialog(mainView);
            };

            desktop.MainWindow = mainView;
            mainView.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}