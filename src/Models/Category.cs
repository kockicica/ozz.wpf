namespace ozz.wpf.Models;

public class Category : HasId {
    public string Name  { get; set; }
    public int    Order { get; set; }

    #region HasId Members

    public int Id { get; set; }

    #endregion

}