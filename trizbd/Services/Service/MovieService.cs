using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service
{
    public class MovieService : IMovieService
    {
        private readonly CinemaDbContext _context;

        public MovieService(CinemaDbContext context) => _context = context;

        public async Task<List<Movie>> GetAllAsync()
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .ToListAsync();
        }

        public async Task<Movie?> GetByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Movie> AddAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<Movie> UpdateAsync(Movie movie)
        {
            var existing = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.Id == movie.Id);
            if (existing == null) throw new Exception("Фильм не найден");

            existing.Title = movie.Title;
            existing.Description = movie.Description;
            existing.DurationMinutes = movie.DurationMinutes;
            existing.ReleaseDate = movie.ReleaseDate;

            _context.Movies.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Movie>> GetByGenreAsync(int genreId)
        {
            return await _context.Movies
                .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .ToListAsync();
        }
    }
}
