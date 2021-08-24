namespace middlerApp.Auth.Entities
{
    public interface IConcurrencyAware
    {
        string ConcurrencyStamp { get; set; }
    }
}
