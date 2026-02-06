namespace trizbd.Classes;

public class Seat:ObservableObject
{
    #region Properties

    private int _id;
    private int _hallId;
    private int _row;
    private int _number;

    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public int HallId { get=>_hallId; set=>SetProperty(ref _hallId,value); }
    public int Row { get=>_row; set=>SetProperty(ref _row,value); }
    public int Number { get=>_number; set=>SetProperty(ref _number,value); }

    public Hall Hall { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}