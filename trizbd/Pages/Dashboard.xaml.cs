using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using trizbd;
using trizbd.Classes;
using trizbd.Data;

namespace trizbd.Pages;

public partial class Dashboard : Page
{
    public Dashboard()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var from = DateTime.Today;
            var to = from.AddDays(1);

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var soldTodayCount = await db.Tickets.AsNoTracking()
                .CountAsync(t => t.Status == TicketStatus.Sold && t.SaleTime >= from && t.SaleTime < to);

            var revenueToday = await db.Tickets.AsNoTracking()
                .Where(t => t.Status == TicketStatus.Sold && t.SaleTime >= from && t.SaleTime < to)
                .SumAsync(t => (decimal?)t.Price) ?? 0m;

            var sessionsToday = await db.Sessions.AsNoTracking()
                .CountAsync(s => s.StartTime >= from && s.StartTime < to);

            SoldTodayTb.Text = soldTodayCount.ToString();
            RevenueTodayTb.Text = $"{revenueToday:N0} ₽";
            ActiveSessionsTb.Text = sessionsToday.ToString();

            SoldTodayHintTb.Text = soldTodayCount > 0 ? "+ за сегодня" : string.Empty;
            RevenueTodayHintTb.Text = revenueToday > 0 ? "+ за сегодня" : string.Empty;

            var recentTickets = await db.Tickets.AsNoTracking()
                .Where(t => t.Status == TicketStatus.Sold)
                .Include(t => t.Seat)
                .Include(t => t.Session).ThenInclude(s => s.Movie)
                .Include(t => t.Session).ThenInclude(s => s.Hall)
                .OrderByDescending(t => t.SaleTime)
                .Take(10)
                .ToListAsync();

            RecentSalesGrid.ItemsSource = recentTickets
                .Select(t => new RecentSaleRow
                {
                    SaleTime = t.SaleTime.ToString("HH:mm"),
                    Movie = t.Session.Movie.Title,
                    Hall = RuText.HallName(t.Session.Hall.Name),
                    Seat = $"{t.Seat.Row}-{t.Seat.Number}",
                    Price = $"{t.Price:N0} ₽"
                })
                .ToList();
        }
        catch
        {
            SoldTodayTb.Text = "—";
            RevenueTodayTb.Text = "—";
            ActiveSessionsTb.Text = "—";
            SoldTodayHintTb.Text = string.Empty;
            RevenueTodayHintTb.Text = string.Empty;
            RecentSalesGrid.ItemsSource = new List<RecentSaleRow>();
        }
    }

    private sealed class RecentSaleRow
    {
        public string SaleTime { get; set; } = string.Empty;
        public string Movie { get; set; } = string.Empty;
        public string Hall { get; set; } = string.Empty;
        public string Seat { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
