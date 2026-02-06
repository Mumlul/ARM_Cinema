namespace trizbd.Classes;

public class Cinema:ObservableObject
{
    #region Properties

    private int _id;
    private string _name;
    private string _address;

    #endregion
    
    
    public int Id { get => _id; set=>SetProperty(ref _id, value); }
    public string Name { get=>_name; set=>SetProperty(ref _name,value); }
    public string Address { get=>_address; set=>SetProperty(ref _address,value); }

    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
}