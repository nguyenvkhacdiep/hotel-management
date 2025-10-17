namespace HotelManagement.Services.Dto;

public class AddServiceDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
}

public class ServiceResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}