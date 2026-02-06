using trizbd.Classes;

namespace trizbd.Services.Interfaces;

public interface IPaymentService
{
    Task<Payment> AddPaymentAsync(Payment payment);
    Task<Payment?> GetByTicketIdAsync(int ticketId);
    Task<List<Payment>> GetAllAsync();
}