using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using trizbd.Classes;
using trizbd.Data;

namespace trizbd.Pages;

public partial class Ticket_Sale : Page
{
    // Кэшируем схемы, чтобы не читать JSON каждый раз.
    private static readonly Dictionary<int, int[][]> LayoutCache = new();

    private readonly List<SessionVm> _sessions = new();
    private readonly List<SeatVm> _seats = new();

    private bool _isInitialized;

    public Ticket_Sale()
    {
        InitializeComponent();
        Loaded += async (_, _) => await EnsureLoadedAsync();
    }

    /*protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await EnsureLoadedAsync();
        ApplyPendingSessionSelection();
    }*/

    private async Task EnsureLoadedAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        PaymentMethodCb.SelectedIndex = 0;
        await LoadSessionsAsync();
        ApplyPendingSessionSelection();
    }

    private void ApplyPendingSessionSelection()
    {
        var pending = AppState.PendingSessionId;
        if (pending == null) return;

        var idx = _sessions.FindIndex(s => s.Id == pending.Value);
        if (idx >= 0)
        {
            SessionsCb.SelectedIndex = idx;
        }

        AppState.PendingSessionId = null;
    }

    private async Task LoadSessionsAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var from = DateTime.Today;
            var to = from.AddDays(1);

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Where(s => s.StartTime >= from && s.StartTime < to)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            _sessions.Clear();
            foreach (var s in sessions)
            {
                var hallRu = RuText.HallName(s.Hall.Name);
                _sessions.Add(new SessionVm
                {
                    Id = s.Id,
                    HallId = s.HallId,
                    StartTime = s.StartTime,
                    BasePrice = s.BasePrice,
                    MovieTitle = s.Movie.Title,
                    HallName = hallRu,
                    Display = $"{s.StartTime:HH:mm}  •  {s.Movie.Title}  •  {hallRu}  •  {s.BasePrice:N0} ₽"
                });
            }

            SessionsCb.ItemsSource = _sessions;
            SessionsCb.SelectedIndex = _sessions.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить сеансы.\n" + ex.Message);
        }
    }

    private async void SessionsCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await LoadSeatsAsync();
    }

    private async Task LoadSeatsAsync()
    {
        try
        {
            var session = SessionsCb.SelectedItem as SessionVm;
            if (session == null)
            {
                _seats.Clear();
                SeatsIc.ItemsSource = null;
                UpdateSummary();
                return;
            }

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var layout = LoadHallLayout(session.HallId);
            var rows = layout.Length;
            var cols = layout[0].Length;

            var seatsInDb = await db.Seats
                .Where(s => s.HallId == session.HallId)
                .ToListAsync();

            var byPos = seatsInDb
                .GroupBy(s => (s.Row, s.Number))
                .ToDictionary(g => g.Key, g => g.First());

            var added = 0;
            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    if (layout[r][c] != 1) continue;

                    var rowNum = r + 1;
                    var seatNum = c + 1;
                    if (byPos.ContainsKey((rowNum, seatNum))) continue;

                    db.Seats.Add(new Seat
                    {
                        HallId = session.HallId,
                        Row = rowNum,
                        Number = seatNum
                    });
                    added++;
                }
            }

            if (added > 0)
            {
                await db.SaveChangesAsync();
                seatsInDb = await db.Seats
                    .Where(s => s.HallId == session.HallId)
                    .ToListAsync();

                byPos = seatsInDb
                    .GroupBy(s => (s.Row, s.Number))
                    .ToDictionary(g => g.Key, g => g.First());
            }

            var occupied = await db.Tickets
                .AsNoTracking()
                .Where(t => t.SessionId == session.Id && t.Status == TicketStatus.Sold)
                .Select(t => t.SeatId)
                .ToListAsync();

            _seats.Clear();

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var rowNum = r + 1;
                    var seatNum = c + 1;

                    if (layout[r][c] != 1)
                    {
                        _seats.Add(new SeatVm
                        {
                            SeatId = null,
                            Row = rowNum,
                            Number = seatNum,
                            IsVoid = true,
                            IsOccupied = false,
                            IsSelected = false
                        });
                        continue;
                    }

                    byPos.TryGetValue((rowNum, seatNum), out var seat);
                    var seatId = seat?.Id;

                    _seats.Add(new SeatVm
                    {
                        SeatId = seatId,
                        Row = rowNum,
                        Number = seatNum,
                        IsVoid = seatId == null,
                        IsOccupied = seatId != null && occupied.Contains(seatId.Value),
                        IsSelected = false
                    });
                }
            }

            SeatsIc.ItemsSource = _seats;
            UpdateSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить места.\n" + ex.Message);
        }
    }

    private static int[][] LoadHallLayout(int hallId)
    {
        if (LayoutCache.TryGetValue(hallId, out var cached))
            return cached;

        var baseDir = AppContext.BaseDirectory;
        var dirs = new[]
        {
            Path.Combine(baseDir, "Assets", "Halls"),
            baseDir
        };

        string? filePath = null;
        foreach (var dir in dirs.Distinct())
        {
            if (!Directory.Exists(dir)) continue;
            filePath = Directory.EnumerateFiles(dir, $"Hall_{hallId}_*.json", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            if (filePath != null) break;
        }

        if (filePath == null)
        {
            var tryDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            if (Directory.Exists(tryDir))
            {
                filePath = Directory.EnumerateFiles(tryDir, $"Hall_{hallId}_*.json", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault();
            }
        }

        int[][] layout;
        try
        {
            if (filePath != null && File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                layout = JsonSerializer.Deserialize<int[][]>(json) ?? BuildFallbackLayout();
            }
            else
            {
                layout = BuildFallbackLayout();
            }
        }
        catch
        {
            layout = BuildFallbackLayout();
        }

        layout = NormalizeTo20x20(layout);
        LayoutCache[hallId] = layout;
        return layout;
    }

    private static int[][] NormalizeTo20x20(int[][] input)
    {
        var rows = 20;
        var cols = 20;
        var result = new int[rows][];
        for (var r = 0; r < rows; r++)
        {
            result[r] = new int[cols];
            for (var c = 0; c < cols; c++)
            {
                var v = 0;
                if (r < input.Length && input[r] != null && c < input[r].Length)
                    v = input[r][c];
                result[r][c] = v == 1 ? 1 : 0;
            }
        }
        return result;
    }

    private static int[][] BuildFallbackLayout()
    {
        var layout = new int[20][];
        for (var r = 0; r < 20; r++)
        {
            layout[r] = new int[20];
            for (var c = 0; c < 20; c++)
            {
                var isAisle = c == 9 || c == 10;
                layout[r][c] = isAisle ? 0 : 1;
            }
        }
        for (var c = 0; c < 20; c++) layout[0][c] = 0;
        return layout;
    }

    private void Seat_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not SeatVm vm) return;
        if (vm.IsVoid || vm.SeatId == null) return;
        if (vm.IsOccupied) return;

        vm.IsSelected = !vm.IsSelected;
        SeatsIc.Items.Refresh();
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var session = SessionsCb.SelectedItem as SessionVm;

        var count = _seats.Count(s => s.IsSelected);
        SelectedCountTb.Text = count.ToString();

        var total = session == null ? 0m : count * session.BasePrice;
        TotalTb.Text = $"{total:N0} ₽";
    }

    private void ClearSelectionBtn_Click(object sender, RoutedEventArgs e)
    {
        foreach (var s in _seats)
            if (!s.IsVoid && !s.IsOccupied) s.IsSelected = false;

        SeatsIc.Items.Refresh();
        UpdateSummary();
    }

    private async void SellBtn_Click(object sender, RoutedEventArgs e)
    {
        var session = SessionsCb.SelectedItem as SessionVm;
        if (session == null)
        {
            MessageBox.Show("Выберите сеанс");
            return;
        }

        var selected = _seats.Where(s => s.IsSelected && !s.IsOccupied && !s.IsVoid && s.SeatId != null).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Выберите места");
            return;
        }

        if (!long.TryParse(PhoneTb.Text?.Trim(), out var phone))
        {
            MessageBox.Show("Введите корректный телефон (только цифры)");
            return;
        }

        var fullName = NameTb.Text?.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Введите ФИО клиента");
            return;
        }

        var emp = AppState.CurrentEmployee;
        if (emp == null)
        {
            MessageBox.Show("Нет текущего сотрудника. Перелогиньтесь.");
            return;
        }

        var paymentMethod = (PaymentMethodCb.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Карта";

        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            await using var tx = await db.Database.BeginTransactionAsync();

            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Phone == phone);
            if (customer == null)
            {
                customer = new Customer { FullName = fullName!, Phone = phone };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }
            else
            {
                if (!string.Equals(customer.FullName, fullName, StringComparison.OrdinalIgnoreCase))
                    customer.FullName = fullName!;
            }

            var soldCount = 0;
            foreach (var seat in selected)
            {
                var seatId = seat.SeatId!.Value;
                var alreadySold = await db.Tickets.AnyAsync(t =>
                    t.SessionId == session.Id && t.SeatId == seatId && t.Status == TicketStatus.Sold);

                if (alreadySold)
                    continue;

                var ticket = new Ticket
                {
                    SessionId = session.Id,
                    SeatId = seatId,
                    CustomerId = customer.Id,
                    EmployeeId = emp.Id,
                    Price = session.BasePrice,
                    Status = TicketStatus.Sold,
                    SaleTime = DateTime.Now
                };

                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();

                var payment = new Payment
                {
                    TicketId = ticket.Id,
                    PaymentMethod = paymentMethod,
                    Amount = ticket.Price,
                    PaymentTime = DateTime.Now
                };

                db.Payments.Add(payment);
                await db.SaveChangesAsync();

                soldCount++;
            }

            await tx.CommitAsync();

            MessageBox.Show(soldCount > 0
                ? $"Продано билетов: {soldCount} ✅"
                : "Выбранные места уже были проданы другим оператором.");

            PhoneTb.Clear();
            NameTb.Clear();
            await LoadSeatsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка продажи:\n" + ex.Message);
        }
    }

    private sealed class SessionVm
    {
        public int Id { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public decimal BasePrice { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    private sealed class SeatVm
    {
        public int? SeatId { get; set; }
        public int Row { get; set; }
        public int Number { get; set; }
        public bool IsVoid { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsSelected { get; set; }

        public string Label => IsVoid ? string.Empty : Number.ToString();
        public string Tooltip => IsVoid ? string.Empty : $"Ряд {Row}, место {Number}";
    }
}
