namespace trizbd.Classes;

public class MovieGenre:ObservableObject
{
    #region Properties

    private int _movieId;
    private int _genreId;

    #endregion
    
    
    public int MovieId { get=>_movieId; set=>SetProperty(ref _movieId,value); }
    public int GenreId { get=>_genreId; set=>SetProperty(ref _genreId,value); }

    public Movie Movie { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}