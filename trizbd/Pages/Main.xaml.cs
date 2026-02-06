using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using trizbd;
using trizbd.Classes;

namespace trizbd.Pages;

public partial class Main : Page
{
    private readonly Dictionary<string, Page> _pages = new();

    public Main()
    {
        InitializeComponent();

        Loaded += Main_Loaded;
    }

    private void Main_Loaded(object sender, RoutedEventArgs e)
    {
        AppState.MainFrame = ContentFrame;
        AppState.MainPage = this;

        var emp = AppState.CurrentEmployee;
        UserNameTb.Text = emp?.FullName ?? "Гость";
        UserRoleTb.Text = emp?.Role == EmployeeRole.Admin ? "АДМИНИСТРАТОР" : "ОПЕРАТОР";

        ThemeToggleBtn.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";

        NavAdmin.Visibility = emp?.Role == EmployeeRole.Admin ? Visibility.Visible : Visibility.Collapsed;

        _pages["Dashboard"] = new Dashboard();
        _pages["Schedule"] = new Schedule();
        _pages["Tickets"] = new Ticket_Sale();
        _pages["Movies"] = new Movies();
        _pages["Reports"] = new Reports();
        _pages["Admin"] = new Admin_Panel();

        NavigateTo("Dashboard");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var key = btn.Tag?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(key)) return;

        NavigateTo(key);
    }

    public void NavigateTo(string key)
    {
        if (!_pages.TryGetValue(key, out var page))
            return;
        ContentFrame.Navigate(page);
        SetActiveNav(key);
    }

    private void SetActiveNav(string key)
    {
        var all = new[]
        {
            ("Dashboard", NavDashboard),
            ("Schedule", NavSchedule),
            ("Tickets", NavTickets),
            ("Movies", NavMovies),
            ("Reports", NavReports),
            ("Admin", NavAdmin)
        };

        foreach (var (k, b) in all)
        {
            if (b == null) continue;
            b.Style = (Style)Application.Current.Resources[k == key ? "NavButtonActiveStyle" : "NavButtonStyle"];
        }
    }



    private void ThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        ThemeManager.Toggle();
        ThemeToggleBtn.Content = ThemeManager.CurrentTheme == AppTheme.Dark ? "Светлая тема" : "Тёмная тема";
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        AppState.CurrentEmployee = null;

        NavigationService?.Navigate(App.Services.GetRequiredService<Login>());
    }
}
