using trizbd.Classes;

namespace trizbd.Services.Interfaces;

public interface ITicketService
{
    Task<List<Ticket>> GetTicketsBySessionAsync(int sessionId);
    Task<Ticket> SellTicketAsync(Ticket ticket);
    Task<bool> IsSeatAvailableAsync(int sessionId, int seatId);
}