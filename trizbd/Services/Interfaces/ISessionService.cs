using trizbd.Classes;

namespace trizbd.Services.Interfaces;

public interface ISessionService
{
    Task<List<Session>> GetAllAsync();
    Task<List<Session>> GetByMovieAsync(int movieId);
    Task<Session?> GetByIdAsync(int id);
    Task<Session> AddAsync(Session session);
    Task<Session> UpdateAsync(Session session);
    Task DeleteAsync(int id);
}