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

public partial class Reports : Page
{
    public Reports()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            FromDp.SelectedDate ??= DateTime.Today;
            ToDp.SelectedDate ??= DateTime.Today.AddDays(1);
            await LoadAsync();
        };
    }

    private async Task LoadAsync()
    {
        try
        {
            var from = (FromDp.SelectedDate ?? DateTime.Today).Date;
            var to = (ToDp.SelectedDate ?? DateTime.Today.AddDays(1)).Date;
            if (to <= from) to = from.AddDays(1);

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var ticketsQuery = db.Tickets.AsNoTracking()
                .Include(t => t.Session).ThenInclude(s => s.Movie)
                .Where(t => t.Status == TicketStatus.Sold && t.SaleTime >= from && t.SaleTime < to);

            var soldCount = await ticketsQuery.CountAsync();
            var revenue = await ticketsQuery.SumAsync(t => (decimal?)t.Price) ?? 0m;
            var avg = soldCount == 0 ? 0m : revenue / soldCount;

            SoldTb.Text = soldCount.ToString();
            RevenueTb.Text = $"{revenue:N0} ₽";
            AvgTb.Text = $"{avg:N0} ₽";

            var top = await ticketsQuery
                .GroupBy(t => t.Session.Movie.Title)
                .Select(g => new TopMovieRow
                {
                    Movie = g.Key,
                    Tickets = g.Count(),
                    Revenue = g.Sum(x => x.Price)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(20)
                .ToListAsync();

            TopMoviesGrid.ItemsSource = top;
            EmptyTb.Visibility = top.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка формирования отчёта:\n" + ex.Message);
        }
    }

    private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private sealed class TopMovieRow
    {
        public string Movie { get; set; } = string.Empty;
        public int Tickets { get; set; }
        public decimal Revenue { get; set; }
    }
}
