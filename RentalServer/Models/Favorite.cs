namespace RentalServer.Models;

public class Favorite
{
    public Guid Id { get; set; }
    

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;


    public Guid RentalId { get; set; }
    public Rental Rental { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}