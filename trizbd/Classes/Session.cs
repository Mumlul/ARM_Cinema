namespace trizbd.Classes;

public class Session:ObservableObject
{
    #region Properties

    private int _id;
    private int _movieId;
    private int _hallId;
    private DateTime _date;
    private decimal _price;

    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public int MovieId { get=>_movieId; set=>SetProperty(ref _movieId,value); }
    public int HallId { get=>_hallId; set=>SetProperty(ref _hallId,value); }
    public DateTime StartTime { get=>_date; set=>SetProperty(ref _date,value); }
    public decimal BasePrice { get=>_price; set=>SetProperty(ref _price,value); }

    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}