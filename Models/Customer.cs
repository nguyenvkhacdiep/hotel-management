namespace HotelManagement.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string IDCard { get; set; } = null!;
   
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}