using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service;

public class CustomerService:ICustomerService
{
    private readonly CinemaDbContext  _context;
    
    public CustomerService(CinemaDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        var existing = await _context.Customers.FindAsync(customer.Id);
        if (existing == null)
            throw new Exception("Клиент не найден");

        existing.FullName = customer.FullName;
        existing.Phone = customer.Phone;

        _context.Customers.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return;

        var hasTickets = await _context.Tickets.AnyAsync(t => t.CustomerId == id);
        if (hasTickets)
            throw new InvalidOperationException("Нельзя удалить клиента с купленными билетами");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<Customer?> FindByPhoneAsync(long phone)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }
}