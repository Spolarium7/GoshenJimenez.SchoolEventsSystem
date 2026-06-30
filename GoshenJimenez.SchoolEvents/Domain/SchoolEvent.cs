public class SchoolEvent
{
    public SchoolEvent(string name, DateTime dateStart, int? durationInDays) //METHOD, CONSTRUCTOR
    {
        Id = Guid.NewGuid();
        Name = name;
        DateStart = dateStart;
        DurationInDays = durationInDays;
        //DateEnd = dateStart.AddDays(durationInDays ?? 0);
    }
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public DateTime DateStart { get; set; }
    public int? DurationInDays { get; set; }
    public void IncreaseDuration(int days)
    {
        DurationInDays = (DurationInDays ?? 0) + days;
    }
    public void DecreaseDuration(int days)
    {
        DurationInDays = (DurationInDays ?? 0) - days;
    }
    public DateTime DateEnd 
    {        
        get
        {
            return DateStart.AddDays(DurationInDays ?? 0);
        } 
    } 
}