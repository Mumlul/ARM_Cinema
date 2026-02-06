using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service;

public class PaymentService : IPaymentService
{
    private readonly CinemaDbContext _context;

    public PaymentService(CinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetByTicketIdAsync(int ticketId)
    {
        return await _context.Payments
            .Include(p => p.Ticket)
            .FirstOrDefaultAsync(p => p.TicketId == ticketId);
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        return await _context.Payments
            .Include(p => p.Ticket)
            .ToListAsync();
    }
}
