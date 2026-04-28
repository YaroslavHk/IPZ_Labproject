namespace RentalServer.Models;

public class RentalImage
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    
    public string FullImageUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    public bool IsMain { get; set; }
}