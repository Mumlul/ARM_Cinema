using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using trizbd.Classes;
using trizbd.Data;

namespace trizbd.Windows;

public partial class AddSessionWindow : Window
{
    private const int StepMinutes = 15;
    private const int OpenHour = 10;     // рекомендованное начало дня
    private const int CloseHour = 23;    // рекомендованное окончание дня

    private int? _preselectMovieId;

    public AddSessionWindow(int? preselectMovieId = null)
    {
        InitializeComponent();
        _preselectMovieId = preselectMovieId;

        Loaded += async (_, _) =>
        {
            DateDp.SelectedDate ??= DateTime.Today;
            PriceTb.Text = "500";
            await LoadDictionariesAsync();
            await RecalcSuggestionsAsync();
            await UpdateHintAsync();
        };
    }

    private async Task LoadDictionariesAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var movies = await db.Movies
                .AsNoTracking()
                .OrderBy(m => m.Title)
                .ToListAsync();

            var halls = await db.Halls
                .AsNoTracking()
                .OrderBy(h => h.Name)
                .ToListAsync();

            MovieCb.ItemsSource = movies;
            HallCb.ItemsSource = halls;

            if (_preselectMovieId.HasValue)
            {
                MovieCb.SelectedItem = movies.FirstOrDefault(m => m.Id == _preselectMovieId.Value);
            }

            MovieCb.SelectedIndex = MovieCb.SelectedIndex >= 0 ? MovieCb.SelectedIndex : (movies.Count > 0 ? 0 : -1);
            HallCb.SelectedIndex = halls.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось загрузить справочники:" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AnyField_Changed(object sender, EventArgs e)
    {
        await RecalcSuggestionsAsync();
        await UpdateHintAsync();
    }

    private async Task RecalcSuggestionsAsync()
    {
        try
        {
            var movie = MovieCb.SelectedItem as Movie;
            var hall = HallCb.SelectedItem as Hall;
            var date = DateDp.SelectedDate?.Date;

            if (movie == null || hall == null || date == null)
            {
                SuggestionsLb.ItemsSource = Array.Empty<string>();
                return;
            }

            var day = date.Value;
            var from = day;
            var to = day.AddDays(1);

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Where(s => s.HallId == hall.Id && s.StartTime >= from && s.StartTime < to)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            var duration = Math.Max(1, movie.DurationMinutes);
            var open = day.AddHours(OpenHour);
            var close = day.AddHours(CloseHour);

            var suggestions = new List<string>();
            for (var t = open; t <= close.AddMinutes(-duration); t = t.AddMinutes(StepMinutes))
            {
                var candStart = t;
                var candEnd = candStart.AddMinutes(duration);

                bool overlaps = sessions.Any(s =>
                {
                    var sStart = s.StartTime;
                    var sEnd = sStart.AddMinutes(Math.Max(1, s.Movie.DurationMinutes));
                    return candStart < sEnd && candEnd > sStart;
                });

                if (!overlaps)
                    suggestions.Add(candStart.ToString("HH:mm"));
            }

            // если зал полностью свободен — подскажем начальные варианты
            if (suggestions.Count == 0 && sessions.Count == 0)
            {
                suggestions.Add(open.ToString("HH:mm"));
                suggestions.Add(open.AddMinutes(60).ToString("HH:mm"));
                suggestions.Add(open.AddMinutes(120).ToString("HH:mm"));
            }

            SuggestionsLb.ItemsSource = suggestions;
        }
        catch
        {
            // подсказки — вспомогательная функция; не валим окно из‑за ошибок подсказок
            SuggestionsLb.ItemsSource = Array.Empty<string>();
        }
    }

    private async Task UpdateHintAsync()
    {
        HintTb.Visibility = Visibility.Collapsed;

        var movie = MovieCb.SelectedItem as Movie;
        var hall = HallCb.SelectedItem as Hall;

        if (movie == null || hall == null)
            return;

        if (!TryGetStart(out var start, out _))
            return;

        var end = start.AddMinutes(Math.Max(1, movie.DurationMinutes));

        try
        {
            var day = start.Date;
            var from = day;
            var to = day.AddDays(1);

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Where(s => s.HallId == hall.Id && s.StartTime >= from && s.StartTime < to)
                .ToListAsync();

            var conflict = sessions.FirstOrDefault(s =>
            {
                var sStart = s.StartTime;
                var sEnd = sStart.AddMinutes(Math.Max(1, s.Movie.DurationMinutes));
                return start < sEnd && end > sStart;
            });

            HintTb.Visibility = Visibility.Visible;

            if (conflict == null)
            {
                HintTb.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
                HintTb.Text = $"Время свободно. Окончание: {end:HH:mm}";
            }
            else
            {
                HintTb.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                var cEnd = conflict.StartTime.AddMinutes(Math.Max(1, conflict.Movie.DurationMinutes));
                HintTb.Text = $"Конфликт: в зале уже идёт сеанс "+conflict.Movie.Title+" ({conflict.StartTime:HH:mm}–{cEnd:HH:mm})";
            }
        }
        catch
        {
            // игнор
        }
    }

    private void SuggestionsLb_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ничего, кнопкой подставим
    }

    private void UseTimeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (SuggestionsLb.SelectedItem is string t && !string.IsNullOrWhiteSpace(t))
        {
            TimeTb.Text = t;
        }
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        var movie = MovieCb.SelectedItem as Movie;
        var hall = HallCb.SelectedItem as Hall;

        if (movie == null || hall == null)
        {
            MessageBox.Show("Выберите фильм и зал.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!TryGetStart(out var start, out var err))
        {
            MessageBox.Show(err, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(PriceTb.Text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
        {
            // попробуем русскую культуру
            if (!decimal.TryParse(PriceTb.Text?.Trim(), NumberStyles.Number, new CultureInfo("ru-RU"), out price))
            {
                MessageBox.Show("Введите корректную цену.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        if (price <= 0)
        {
            MessageBox.Show("Цена должна быть больше нуля.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var end = start.AddMinutes(Math.Max(1, movie.DurationMinutes));

        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var day = start.Date;
            var from = day;
            var to = day.AddDays(1);

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Movie)
                .Where(s => s.HallId == hall.Id && s.StartTime >= from && s.StartTime < to)
                .ToListAsync();

            var conflict = sessions.FirstOrDefault(s =>
            {
                var sStart = s.StartTime;
                var sEnd = sStart.AddMinutes(Math.Max(1, s.Movie.DurationMinutes));
                return start < sEnd && end > sStart;
            });

            if (conflict != null)
            {
                var cEnd = conflict.StartTime.AddMinutes(Math.Max(1, conflict.Movie.DurationMinutes));
                MessageBox.Show(
                    "Нельзя создать сеанс: пересечение по времени.\n" + "Зал занят сеансом "+conflict.Movie.Title+" ({conflict.StartTime:HH:mm}–{cEnd:HH:mm}).\n\n" +
                    $"Выберите другое время (можно из списка подсказок).",
                    "Конфликт расписания",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var newSession = new Session
            {
                MovieId = movie.Id,
                HallId = hall.Id,
                StartTime = start,
                BasePrice = price
            };

            db.Sessions.Add(newSession);
            await db.SaveChangesAsync();

            MessageBox.Show("Сеанс создан ✅", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось создать сеанс:" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool TryGetStart(out DateTime start, out string error)
    {
        start = default;
        error = "Введите дату и время в формате чч:мм.";

        var date = DateDp.SelectedDate?.Date;
        if (date == null)
        {
            error = "Выберите дату.";
            return false;
        }

        var rawTime = TimeTb.Text?.Trim();
        if (string.IsNullOrWhiteSpace(rawTime))
        {
            error = "Введите время начала (например 18:30).";
            return false;
        }

        if (!TimeSpan.TryParseExact(rawTime, "hh:mm", CultureInfo.InvariantCulture, out var time) &&
            !TimeSpan.TryParse(rawTime, CultureInfo.GetCultureInfo("ru-RU"), out time) &&
            !TimeSpan.TryParse(rawTime, out time))
        {
            error = "Неверный формат времени. Пример: 18:30";
            return false;
        }

        start = date.Value.Add(time);

        if (start.Year < 2000 || start.Year > 2100)
        {
            error = "Некорректная дата.";
            return false;
        }

        return true;
    }
}
