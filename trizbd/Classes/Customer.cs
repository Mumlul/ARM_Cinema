namespace trizbd.Classes;

public class Customer:ObservableObject
{
    #region Properties

    private int _id;
    private string _fullName;
    private long _phone;
    private string _email;
    
    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public string FullName { get=>_fullName; set=>SetProperty(ref _fullName,value); } 
    public long Phone { get=>_phone; set=>SetProperty(ref _phone,value); }
    public string? Email { get=>_email; set=>SetProperty(ref _email,value); }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}