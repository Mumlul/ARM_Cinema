using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using trizbd.Classes;
using trizbd.Data;

namespace trizbd.Pages;

public partial class Admin_Panel : Page
{
    private readonly List<EmployeeRow> _rows = new();

    public Admin_Panel()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
        RoleCb.SelectedIndex = 0;
        IsActiveCb.IsChecked = true;
    }

    private async Task LoadAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var employees = await db.Employees.AsNoTracking()
                .OrderByDescending(e => e.Role)
                .ThenBy(e => e.FullName)
                .ToListAsync();

            _rows.Clear();
            _rows.AddRange(employees.Select(e => new EmployeeRow
            {
                Id = e.Id,
                FullName = e.FullName,
                Login = e.Login,
                Role = e.Role == EmployeeRole.Admin ? "Администратор" : "Оператор",
                Active = e.IsActive ? "Да" : "Нет"
            }));

            EmployeesGrid.ItemsSource = null;
            EmployeesGrid.ItemsSource = _rows;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки сотрудников:\n" + ex.Message);
        }
    }

    private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var fullName = FullNameTb.Text?.Trim();
        var login = LoginTb.Text?.Trim();
        var password = PasswordTb.Password;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Заполните ФИО, логин и пароль");
            return;
        }

        var role = RoleCb.SelectedIndex == 1 ? EmployeeRole.Admin : EmployeeRole.Operator;
        var isActive = IsActiveCb.IsChecked == true;

        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var exists = await db.Employees.AnyAsync(x => x.Login == login);
            if (exists)
            {
                MessageBox.Show("Логин уже существует");
                return;
            }

            var employee = new Employee
            {
                FullName = fullName!,
                Login = login!,
                PasswordHash = password, 
                Role = role,
                IsActive = isActive
            };

            employee.PasswordHash = HashPassword(employee.PasswordHash);

            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            MessageBox.Show("Сотрудник добавлен ✅");

            FullNameTb.Clear();
            LoginTb.Clear();
            PasswordTb.Clear();
            RoleCb.SelectedIndex = 0;
            IsActiveCb.IsChecked = true;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка добавления сотрудника:\n" + ex.Message);
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private sealed class EmployeeRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Active { get; set; } = string.Empty;
    }
}
