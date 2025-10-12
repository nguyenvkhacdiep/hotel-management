using HotelManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<BookingService> BookingServices { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<RoomType> RoomTypes { get; set; } = null!;
    public DbSet<Floor> Floors { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;
    public DbSet<RoomAmenity> RoomAmenities { get; set; } = null!;
    public DbSet<RoomPrice> RoomPrices { get; set; } = null!;
    public DbSet<RoomStatusHistory> RoomStatusHistories { get; set; } = null!;
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<BookingService>()
            .HasKey(bs => new { bs.BookingId, bs.ServiceId });
        
        modelBuilder.Entity<Room>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.PaymentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.User)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasOne(e => e.RoomType)
                .WithMany(rt => rt.Rooms)
                .HasForeignKey(e => e.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<RoomAmenity>(entity =>
        {
            entity.HasOne(e => e.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(e => e.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<RoomPrice>().HasOne(e => e.Room)
            .WithMany(r => r.RoomPrices)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RoomStatusHistory>().HasOne(e => e.Room)
            .WithMany(r => r.StatusHistories)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Amenity>().Property(e => e.Category)
            .HasConversion<string>();
        
        modelBuilder.Entity<RoomPrice>().Property(e => e.DayType)
            .HasConversion<string>();
        
        modelBuilder.Entity<RoomPrice>().Property(e => e.SeasonName)
            .HasConversion<string>();
    }
}
