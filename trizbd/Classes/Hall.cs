namespace trizbd.Classes;

public class Hall:ObservableObject
{
    #region Properties
    
    private int _id;
    private int _cinemaId;
    private string _name;
    private int _totalSeats;

    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public int CinemaId { get=>_cinemaId; set=>SetProperty(ref _cinemaId,value); }
    public string Name { get=>_name; set=>SetProperty(ref _name,value); }
    public int TotalSeats { get=>_totalSeats; set=>SetProperty(ref _totalSeats,value); }

    public Cinema Cinema { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}