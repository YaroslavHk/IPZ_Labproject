namespace RentalServer.DTO;

public class AuthResponse
{
    public string Token { get; set; }
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }
    
    public string Email { get; set; }
    
    public string phone { get; set; }
}

public class AuthRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
}

public class registerRequest
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
}

public class UpdateProfileRequest
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null;
    public string Email { get; set; } = null;
    public string Phone { get; set; } = null;
}


