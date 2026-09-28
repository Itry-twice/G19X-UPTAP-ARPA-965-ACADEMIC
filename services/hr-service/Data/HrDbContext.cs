using Microsoft.EntityFrameworkCore;

namespace PluriOne.HrService.Data;

public sealed class HrDbContext : DbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketStatusHistory> TicketStatusHistory => Set<TicketStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hr");

        modelBuilder.Entity<Category>().ToTable("categories");
        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        modelBuilder.Entity<Ticket>().ToTable("tickets");
        modelBuilder.Entity<Ticket>().HasIndex(t => t.RequesterUserId);
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId);
        modelBuilder.Entity<Ticket>()
            .HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey(c => c.TicketId);

        modelBuilder.Entity<TicketComment>().ToTable("ticket_comments");
        modelBuilder.Entity<TicketComment>().HasIndex(c => c.TicketId);

        modelBuilder.Entity<TicketStatusHistory>().ToTable("ticket_status_history");
        modelBuilder.Entity<TicketStatusHistory>().HasIndex(h => h.TicketId);
        modelBuilder.Entity<TicketStatusHistory>()
            .HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(h => h.TicketId);
    }
}
