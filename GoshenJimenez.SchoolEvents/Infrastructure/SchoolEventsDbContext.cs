using Microsoft.EntityFrameworkCore;

public class SchoolEventsDbContext : DbContext
{
    public SchoolEventsDbContext(DbContextOptions<SchoolEventsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SchoolEvent> SchoolEvents => Set<SchoolEvent>();
    public DbSet<SchoolEventParticipant> SchoolEventParticipants => Set<SchoolEventParticipant>();
    public DbSet<UserLoginInfo> UserLoginInfos => Set<UserLoginInfo>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SchoolEvent>().Ignore(e => e.DateEnd);
    }
}