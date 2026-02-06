using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service
{
    public class TicketService : ITicketService
    {
        private readonly CinemaDbContext _context;

        public TicketService(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> GetTicketsBySessionAsync(int sessionId)
        {
            return await _context.Tickets
                .Where(t => t.SessionId == sessionId)
                .Include(t => t.Seat)
                .Include(t => t.Customer)
                .Include(t => t.Employee)
                .Include(t => t.Payment)
                .ToListAsync();
        }

        public async Task<Ticket> SellTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<bool> IsSeatAvailableAsync(int sessionId, int seatId)
        {
            return !await _context.Tickets
                .AnyAsync(t => t.SessionId == sessionId && t.SeatId == seatId);
        }
    }
}