namespace trizbd.Classes;

public class Genre:ObservableObject
{
    #region MyRegion

    private int _id;
    private string _name;

    #endregion
    
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public string Name { get=>_name; set=>SetProperty(ref _name,value); } 

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}