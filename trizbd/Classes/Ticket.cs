namespace trizbd.Classes;

public enum TicketStatus
{
    Reserved = 0,
    Sold = 1,
    Cancelled = 2
}

public class Ticket:ObservableObject
{
    #region Properties

    private int _id;
    private int _sessionId;
    private int _seatId;
    private int _customerId;
    private int _employeeId;
    private decimal _price;
    private TicketStatus _status;
    private DateTime _saleTime;

    #endregion
    
    
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int SessionId
    {
        get => _sessionId;
        set => SetProperty(ref _sessionId, value);
    }

    public int SeatId
    {
        get => _seatId;
        set => SetProperty(ref _seatId, value);
    }

    public int CustomerId
    {
        get => _customerId;
        set => SetProperty(ref _customerId, value);
    }

    public int EmployeeId
    {
        get => _employeeId;
        set => SetProperty(ref _employeeId, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public TicketStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public DateTime SaleTime
    {
        get => _saleTime;
        set => SetProperty(ref _saleTime, value);
    }
    

    public Session Session { get; set; } = null!;
    public Seat Seat { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Payment? Payment { get; set; }
    
    public Employee Employee { get; set; } = null!;
}