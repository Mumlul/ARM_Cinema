using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using trizbd.Data;
using trizbd.Pages;
using trizbd.Services.Interfaces;
using trizbd.Services.Service;

namespace trizbd;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Тема (тёмная/светлая) — загружаем до открытия окна
        ThemeManager.LoadAndApplySavedTheme();

        var services = new ServiceCollection();

        // ====== Connection string ======
        // Можно переопределить:
        // 1) переменной окружения TRIZBD_CONNECTION_STRING
        // 2) файлом connection.txt рядом с .exe
        const string defaultConnection =
            "Server=HOME-PC\\MSSQLSERVER01;Database=CinemaARM;Trusted_Connection=True;TrustServerCertificate=True";

        var connectionString = GetConnectionString(defaultConnection);

        services.AddDbContext<CinemaDbContext>(options =>
            options.UseSqlServer(connectionString));

        // ====== Services ======
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<IMovieService, MovieService>();
        services.AddTransient<ISessionService, SessionService>();
        services.AddTransient<ITicketService, TicketService>();
        services.AddTransient<IPaymentService, PaymentService>();

        // ====== UI ======
        services.AddTransient<MainWindow>();
        services.AddTransient<Login>();
        services.AddTransient<Main>();

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static string GetConnectionString(string fallback)
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("TRIZBD_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(env))
                return env.Trim();

            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connection.txt");
            if (File.Exists(file))
            {
                var txt = File.ReadAllText(file).Trim();
                if (!string.IsNullOrWhiteSpace(txt))
                    return txt;
            }
        }
        catch
        {
            // ignored
        }

        return fallback;
    }
}
