using Microsoft.EntityFrameworkCore;
using trizbd.Classes;

namespace trizbd.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options) { }

    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

            // ===================== Cinema =====================
            modelBuilder.Entity<Cinema>(entity =>
            {
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            // ===================== Hall =====================
            modelBuilder.Entity<Hall>(entity =>
            {
                entity.HasIndex(h => new { h.CinemaId, h.Name })
                    .IsUnique();

                entity.Property(h => h.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(h => h.Cinema)
                    .WithMany(c => c.Halls)
                    .HasForeignKey(h => h.CinemaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== Seat =====================
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasIndex(s => new { s.HallId, s.Row, s.Number })
                    .IsUnique();

                entity.HasOne(s => s.Hall)
                    .WithMany(h => h.Seats)
                    .HasForeignKey(s => s.HallId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== Movie =====================
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.Property(m => m.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(m => m.Description)
                    .HasMaxLength(1000);
            });

            // ===================== Genre =====================
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.Property(g => g.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(g => g.Name)
                    .IsUnique();
            });

            // ===================== MovieGenre (M:N) =====================
            modelBuilder.Entity<MovieGenre>(entity =>
            {
                entity.HasKey(mg => new { mg.MovieId, mg.GenreId });

                entity.HasOne(mg => mg.Movie)
                    .WithMany(m => m.MovieGenres)
                    .HasForeignKey(mg => mg.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mg => mg.Genre)
                    .WithMany(g => g.MovieGenres)
                    .HasForeignKey(mg => mg.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== Session =====================
            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(s => s.BasePrice)
                    .HasPrecision(10, 2);

                entity.HasOne(s => s.Movie)
                    .WithMany(m => m.Sessions)
                    .HasForeignKey(s => s.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Hall)
                    .WithMany(h => h.Sessions)
                    .HasForeignKey(s => s.HallId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== Customer =====================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(c => c.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.Phone)
                    .IsRequired();
            });

            // ===================== Ticket =====================
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.Property(t => t.Price)
                    .HasPrecision(10, 2);

                // одно место — один билет на сеанс
                entity.HasIndex(t => new { t.SessionId, t.SeatId })
                    .IsUnique();

                // Ticket → Session (CASCADE)
                entity.HasOne(t => t.Session)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ticket → Seat (RESTRICT)
                entity.HasOne(t => t.Seat)
                    .WithMany(s => s.Tickets)
                    .HasForeignKey(t => t.SeatId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Ticket → Customer (RESTRICT)
                entity.HasOne(t => t.Customer)
                    .WithMany(c => c.Tickets)
                    .HasForeignKey(t => t.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(t => t.Employee)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(t => t.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== Payment =====================
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount)
                    .HasPrecision(10, 2);

                entity.HasOne(p => p.Ticket)
                    .WithOne(t => t.Payment)
                    .HasForeignKey<Payment>(p => p.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            
            // ===================== Employee =====================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Login)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Role)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);
            });
    }
}