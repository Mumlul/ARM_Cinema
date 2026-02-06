using trizbd.Classes;

namespace trizbd.Services.Interfaces;

public interface IEmployeeService
{
    Task<Employee?> AuthenticateAsync(string login, string password);
    Task<List<Employee>> GetAllAsync();
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> AddAsync(Employee employee);
    Task<Employee> UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
}