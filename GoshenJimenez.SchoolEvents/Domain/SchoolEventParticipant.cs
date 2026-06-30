//Associative entity between SchoolEvent and Person
public class SchoolEventParticipant
{
    public SchoolEventParticipant() //METHOD, CONSTRUCTOR
    {
        Id = Guid.NewGuid();
    }
    public Guid? Id { get; set; }
    public Guid? SchoolEventId { get; set; }
    public SchoolEvent? SchoolEvent { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
}