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


public partial class Schedule : Page
{
    private readonly List<ScheduleRow> _rows = new();

    public Schedule()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            DateDp.SelectedDate ??= DateTime.Today;
            await LoadAsync();
        };
    }

    private async Task LoadAsync()
    {
        try
        {
            var day = DateDp.SelectedDate?.Date ?? DateTime.Today;
            var from = day;
            var to = day.AddDays(1);

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var seatsByHall = await db.Seats.AsNoTracking()
                .GroupBy(s => s.HallId)
                .Select(g => new { HallId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.HallId, x => x.Count);

            var sessions = await db.Sessions.AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.StartTime >= from && s.StartTime < to)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var soldBySession = await db.Tickets.AsNoTracking()
                .Where(t => t.Status == TicketStatus.Sold && t.Session.StartTime >= from && t.Session.StartTime < to)
                .GroupBy(t => t.SessionId)
                .Select(g => new { SessionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SessionId, x => x.Count);

            _rows.Clear();
            foreach (var s in sessions)
            {
                seatsByHall.TryGetValue(s.HallId, out var totalSeats);
                soldBySession.TryGetValue(s.Id, out var soldSeats);

                totalSeats = totalSeats == 0 ? 1 : totalSeats;
                var free = Math.Max(0, totalSeats - soldSeats);
                var soldOut = free == 0;

                _rows.Add(new ScheduleRow
                {
                    SessionId = s.Id,
                    Time = s.StartTime.ToString("HH:mm"),
                    Movie = s.Movie.Title,
                    Hall = RuText.HallName(s.Hall.Name),
                    Price = $"{s.BasePrice:N0} ₽",
                    SeatsInfo = $"{soldSeats}/{totalSeats}",
                    StatusText = soldOut ? "МЕСТ НЕТ" : "ЕСТЬ МЕСТА",
                    IsSoldOut = soldOut
                });
            }

            ScheduleIc.ItemsSource = _rows;
            EmptyTb.Visibility = _rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки расписания:\n" + ex.Message);
        }
    }

    private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async void DateDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        await LoadAsync();
    }

    private void SellFromScheduleBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not ScheduleRow row) return;

        AppState.PendingSessionId = row.SessionId;
        AppState.MainPage?.NavigateTo("Tickets");
    }

    private sealed class ScheduleRow
    {
        public int SessionId { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Movie { get; set; } = string.Empty;
        public string Hall { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string SeatsInfo { get; set; } = string.Empty;
        public string StatusText { get; set; } = string.Empty;
        public bool IsSoldOut { get; set; }
    }
}
