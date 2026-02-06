namespace trizbd.Classes;

public enum EmployeeRole
{
    Operator = 0,
    Admin = 1
}

public class Employee:ObservableObject
{
    #region Properties

    private int _id;
    private string _fullName;
    private string _login;
    private string _passwordHash;
    private bool _isActive;
    
    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }

    public string FullName { get=>_fullName; set=>SetProperty(ref _fullName,value); }

    public string Login { get=>_login; set=>SetProperty(ref _login,value); } 

    public string PasswordHash { get=>_passwordHash; set=>SetProperty(ref _passwordHash,value); } 

    public EmployeeRole Role { get; set; }

    public bool IsActive { get=>_isActive; set=>SetProperty(ref _isActive,value); }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}