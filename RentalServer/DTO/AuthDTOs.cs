namespace RentalServer.DTO;

public class AuthResponse
{
    public string Token { get; set; }
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
