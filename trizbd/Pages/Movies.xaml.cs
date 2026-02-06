using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Windows;

namespace trizbd.Pages;

public partial class Movies : Page
{
    private readonly List<MovieCardVm> _all = new();

    public Movies()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            ImportMoviesBtn.Visibility = IsAdmin() ? Visibility.Visible : Visibility.Collapsed;
            CreateSessionBtn.Visibility = IsAdmin() ? Visibility.Visible : Visibility.Collapsed;
            await LoadAsync();
        };
    }

    private static bool IsAdmin()
        => AppState.CurrentEmployee?.Role == EmployeeRole.Admin;

    private async Task LoadAsync()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            var movies = await db.Movies
                .AsNoTracking()
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
                .OrderBy(m => m.Title)
                .ToListAsync();

            _all.Clear();
            foreach (var m in movies)
            {
                var genres = m.MovieGenres.Select(g => g.Genre.Name).Distinct().ToList();
                _all.Add(new MovieCardVm
                {
                    MovieId = m.Id,
                    Title = m.Title,
                    Meta = $"{m.DurationMinutes} мин  •  {m.AgeRestriction}+  •  {m.ReleaseDate:yyyy}",
                    Genres = genres.Count > 0 ? string.Join(", ", genres) : "—",
                    Description = string.IsNullOrWhiteSpace(m.Description) ? "Описание отсутствует" : m.Description
                });
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки фильмов:\n" + ex.Message);
        }
    }

    private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = (SearchTb.Text ?? string.Empty).Trim();
        IEnumerable<MovieCardVm> list = _all;

        if (!string.IsNullOrWhiteSpace(q))
        {
            list = list.Where(m =>
                m.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.Genres.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = list.ToList();
        MoviesIc.ItemsSource = filtered;
        CountTb.Text = $"{filtered.Count}";
        EmptyTb.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OpenMovieBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        if (btn.Tag is not MovieCardVm vm) return;

        AppState.PendingMovieId = vm.MovieId;
        AppState.MainFrame?.Navigate(new Movie_Details());
    }

    private async void ImportMoviesBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin())
        {
            MessageBox.Show("Доступ запрещён. Операция доступна только администратору.");
            return;
        }

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var defaultDir = Path.Combine(baseDir, "Assets", "MoviesImport");
            if (!Directory.Exists(defaultDir))
                Directory.CreateDirectory(defaultDir);

            var dlg = new OpenFileDialog
            {
                Title = "Выберите файл(ы) фильма для импорта",
                Filter = "Файлы фильмов (*.json)|*.json|Все файлы (*.*)|*.*",
                Multiselect = true,
                InitialDirectory = defaultDir
            };

            if (dlg.ShowDialog() != true)
                return;

            int imported = 0;
            int skipped = 0;

            using var scope = App.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();

            foreach (var file in dlg.FileNames)
            {
                MovieImportDto? dto;
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    dto = JsonSerializer.Deserialize<MovieImportDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch
                {
                    skipped++;
                    continue;
                }

                if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
                {
                    skipped++;
                    continue;
                }

                var release = dto.ReleaseDate ?? new DateTime(2026, 1, 1);

                // Защита от дублей (название + год)
                var exists = await db.Movies.AnyAsync(m =>
                    m.Title.ToLower() == dto.Title.Trim().ToLower() &&
                    m.ReleaseDate.Year == release.Year);

                if (exists)
                {
                    skipped++;
                    continue;
                }

                var movie = new Movie
                {
                    Title = dto.Title.Trim(),
                    DurationMinutes = dto.DurationMinutes <= 0 ? 100 : dto.DurationMinutes,
                    AgeRestriction = dto.AgeRestriction < 0 ? 0 : dto.AgeRestriction,
                    ReleaseDate = release,
                    Description = string.IsNullOrWhiteSpace(dto.Description)
                        ? "Описание отсутствует"
                        : dto.Description.Trim()
                };

                db.Movies.Add(movie);
                await db.SaveChangesAsync();

                var genres = dto.Genres
                    ?.Select(g => (g ?? string.Empty).Trim())
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                foreach (var gname in genres)
                {
                    var genre = await db.Genres.FirstOrDefaultAsync(g => g.Name.ToLower() == gname.ToLower());
                    if (genre == null)
                    {
                        genre = new Genre { Name = gname };
                        db.Genres.Add(genre);
                        await db.SaveChangesAsync();
                    }

                    db.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genre.Id });
                }

                await db.SaveChangesAsync();
                imported++;
            }

            await LoadAsync();

            MessageBox.Show($"Импорт завершён. Добавлено: {imported}. Пропущено: {skipped}.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка импорта фильмов:\n" + ex.Message);
        }
    }

    private sealed class MovieCardVm
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string Genres { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private sealed class MovieImportDto
    {
        public string? Title { get; set; }
        public int DurationMinutes { get; set; }
        public int AgeRestriction { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public List<string>? Genres { get; set; }
    }


    private async void CreateSessionBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!IsAdmin())
            return;

        // Пытаемся выбрать фильм по текущему поиску (если есть)
        int? preselectId = null;
        if (!string.IsNullOrWhiteSpace(SearchTb.Text))
        {
            var first = _all.FirstOrDefault(m => m.Title.Contains(SearchTb.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            preselectId = first?.MovieId;
        }

        var wnd = new AddSessionWindow(preselectId)
        {
            Owner = Window.GetWindow(this)
        };

        var ok = wnd.ShowDialog();
        if (ok == true)
        {
            // Обновим список (на случай если в карточках показывается расписание/мета)
            await LoadAsync();
        }
    }

}
