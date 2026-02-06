namespace trizbd.Classes;

public class Payment:ObservableObject
{
    #region Properties

    private int _id;
    private int _ticketId;
    private string _pamentMethod;
    private decimal _amount;
    private DateTime _date;

    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public int TicketId { get=>_ticketId; set=>SetProperty(ref _ticketId,value); }
    public string PaymentMethod { get=>_pamentMethod; set=>SetProperty(ref _pamentMethod,value); }
    public decimal Amount { get=>_amount; set=>SetProperty(ref _amount,value); }
    public DateTime PaymentTime { get=>_date; set=>SetProperty(ref _date,value); }

    public Ticket Ticket { get; set; } = null!;
}