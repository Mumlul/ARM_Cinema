using trizbd.Classes;

namespace trizbd.Services.Interfaces;

public interface IMovieService
{
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(int id);
    Task<Movie> AddAsync(Movie movie);
    Task<Movie> UpdateAsync(Movie movie);
    Task DeleteAsync(int id);
    Task<List<Movie>> GetByGenreAsync(int genreId);
}