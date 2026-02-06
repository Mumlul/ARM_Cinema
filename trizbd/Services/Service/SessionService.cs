using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service
{
    public class SessionService : ISessionService
    {
        private readonly CinemaDbContext _context;

        public SessionService(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Session>> GetAllAsync()
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ToListAsync();
        }

        public async Task<List<Session>> GetByMovieAsync(int movieId)
        {
            return await _context.Sessions
                .Where(s => s.MovieId == movieId)
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ToListAsync();
        }

        public async Task<Session?> GetByIdAsync(int id)
        {
            return await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Session> AddAsync(Session session)
        {
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<Session> UpdateAsync(Session session)
        {
            var existing = await _context.Sessions.FindAsync(session.Id);
            if (existing == null) throw new Exception("Сеанс не найден");

            existing.MovieId = session.MovieId;
            existing.HallId = session.HallId;
            existing.StartTime = session.StartTime;
            existing.BasePrice = session.BasePrice;

            _context.Sessions.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return;

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }
}
