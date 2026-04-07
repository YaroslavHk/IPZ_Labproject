namespace RentalServer.DTO;

public class RentalResponse
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    
    public decimal Price { get; set; }
    
    public string City { get; set; } = "";
    public string Address { get; set; } = "";
    
    public string Type { get; set; } = "";
    
    public float LivingSpace{get; set;}
    

    public Guid UserId { get; set; }
    
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
}

public class ShortRentalResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string City { get; set; } = "";
    public string Type { get; set; } = "";
    public string Address { get; set; } = "";
    public float LivingSpace { get; set; }
    public Guid UserId { get; set; }
}

public class RentalRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    
    public decimal Price { get; set; }
    public string City { get; set; } = "";
    
    public string Address { get; set; } = "";
    public string Type { get; set; } = "";
    
    public float LivingSpace { get; set; }
    public Guid UserId { get; set; }
}

public class RentalPostResponse
{
    public Guid Id { get; set; }
}