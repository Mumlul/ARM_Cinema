using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using trizbd;
using trizbd.Services.Interfaces;

namespace trizbd.Pages;

public partial class Login : Page
{
    private readonly IEmployeeService _employeeService;

    public Login(IEmployeeService employeeService)
    {
        InitializeComponent();
        _employeeService = employeeService;

        UsernameTb.Focus();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorTb.Visibility = Visibility.Collapsed;

        var login = UsernameTb.Text?.Trim() ?? string.Empty;
        var password = PasswordPb.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Введите логин и пароль");
            return;
        }

        try
        {
            var emp = await _employeeService.AuthenticateAsync(login, password);
            if (emp == null)
            {
                ShowError("Неверный логин или пароль");
                return;
            }

            AppState.CurrentEmployee = emp;

            NavigationService?.Navigate(App.Services.GetRequiredService<Main>());
        }
        catch (Exception ex)
        {
            ShowError("Ошибка подключения к базе данных. Проверь файл connection.txt (строка подключения).\n" + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        ErrorTb.Text = message;
        ErrorTb.Visibility = Visibility.Visible;
    }
}
