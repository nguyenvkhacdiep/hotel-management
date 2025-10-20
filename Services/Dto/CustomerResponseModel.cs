namespace HotelManagement.Services.Dto;

public class CustomerResponseModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string IDCard { get; set; } = null!;
}