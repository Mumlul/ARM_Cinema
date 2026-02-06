using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using trizbd.Classes;
using trizbd.Data;
using trizbd.Services.Interfaces;

namespace trizbd.Services.Service;

public class EmployeeService:IEmployeeService
{
    private readonly CinemaDbContext  _context;
    
    public EmployeeService(CinemaDbContext context)=>_context = context;
    
    public async Task<Employee?> AuthenticateAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return null;

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Login == login && e.IsActive);

        if (employee == null)
            return null;

        if (VerifyPasswordHash(password, employee.PasswordHash))
            return employee;
        
       

        return null;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await _context.Employees.ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task<Employee> AddAsync(Employee employee)
    {
        employee.PasswordHash = HashPassword(employee.PasswordHash);
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> UpdateAsync(Employee employee)
    {
        var existing = await _context.Employees.FindAsync(employee.Id);
        if (existing == null) throw new Exception("Сотрудник не найден");

        existing.FullName = employee.FullName;
        existing.Login = employee.Login;
        existing.Role = employee.Role;
        existing.IsActive = employee.IsActive;

        if (!string.IsNullOrWhiteSpace(employee.PasswordHash))
        {
            existing.PasswordHash = HashPassword(employee.PasswordHash);
        }

        _context.Employees.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return;

        // Удаление запрещено, если есть проданные билеты
        var hasTickets = await _context.Tickets.AnyAsync(t => t.EmployeeId == id);
        if (hasTickets)
            throw new InvalidOperationException("Нельзя удалить сотрудника, у которого есть проданные билеты");

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }
    
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool VerifyPasswordHash(string password, string storedHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == storedHash;
    }
    
}