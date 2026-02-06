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


public partial class Movie_Details : Page
{
    private int? _movieId;
    private readonly List<SessionRow> _sessions = new();

    public Movie_Details()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            _movieId = AppState.PendingMovieId;
            if (_movieId == null)
            {
                TitleTb.Text = "Фильм не выбран";
                return;
            }

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var movie = await db.Movies
                .AsNoTracking()
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == _movieId.Value);

            if (movie == null)
            {
                TitleTb.Text = "Фильм не найден";
                return;
            }

            var genres = movie.MovieGenres.Select(g => g.Genre.Name).Distinct().ToList();

            TitleTb.Text = movie.Title;
            MetaTb.Text = $"{movie.DurationMinutes} мин  •  {movie.AgeRestriction}+  •  {movie.ReleaseDate:dd.MM.yyyy}";
            GenresTb.Text = genres.Count > 0 ? string.Join(", ", genres) : "—";
            DescriptionTb.Text = string.IsNullOrWhiteSpace(movie.Description) ? "Описание отсутствует" : movie.Description;

            var from = DateTime.Today;
            var to = from.AddDays(7);

            var sessions = await db.Sessions
                .AsNoTracking()
                .Include(s => s.Hall)
                .Where(s => s.MovieId == movie.Id && s.StartTime >= from && s.StartTime < to)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            _sessions.Clear();
            foreach (var s in sessions)
            {
                _sessions.Add(new SessionRow
                {
                    SessionId = s.Id,
                    Time = s.StartTime.ToString("dd.MM  HH:mm"),
                    Hall = RuText.HallName(s.Hall.Name),
                    Price = $"{s.BasePrice:N0} ₽"
                });
            }

            SessionsIc.ItemsSource = _sessions;
            SessionsEmptyTb.Visibility = _sessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки деталей фильма:\n" + ex.Message);
        }
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            AppState.MainPage?.NavigateTo("Movies");
    }

    private void SellFromMovieBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not SessionRow row) return;

        AppState.PendingSessionId = row.SessionId;
        AppState.MainPage?.NavigateTo("Tickets");
    }

    private sealed class SessionRow
    {
        public int SessionId { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Hall { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
    }
}
