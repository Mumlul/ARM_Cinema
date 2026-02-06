namespace trizbd.Classes;

public class Movie:ObservableObject
{
    #region Properties

    private int _id;
    private string _title;
    private int _duration;
    private int _ageRestriction;
    private DateTime _releaseDate;
    private string _description;
    
    #endregion
    
    public int Id { get=>_id; set=>SetProperty(ref _id,value); }
    public string Title { get=>_title; set=>SetProperty(ref _title,value); }
    public int DurationMinutes { get=>_duration; set=>SetProperty(ref _duration,value); }
    public int AgeRestriction { get=>_ageRestriction; set=>SetProperty(ref _ageRestriction,value); }
    public DateTime ReleaseDate { get=>_releaseDate; set=>SetProperty(ref _releaseDate,value); }
    public string Description { get=>_description; set=>SetProperty(ref _description,value); }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}