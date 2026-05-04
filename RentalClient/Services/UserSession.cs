namespace RentalClient.Services;

public interface IUserSession
{
    string Token { get; }
    bool IsAuthenticated { get; }
    string CurrentUsername { get; }    
    string Email { get; } 
    string Phone { get; }
    Guid UserId { get; }
    void Login(string token, string username, string email, string phone, Guid userId);
    void Logout();
    void UpdateUserInfo(string username, string email, string phone);
}

public class UserSession : IUserSession
{
    public string Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    public string CurrentUsername { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public Guid UserId { get; private set; }
    
    
    public void Login(string token, string username, string email, string phone, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Токен не может быть пустым.");

        Token = token;
        CurrentUsername = username;
        Email = email;
        Phone = phone;
        UserId = userId;
    }

    public void Logout()
    {
        Token = null;
        CurrentUsername = null;
        Email = null;
        Phone = null;
        UserId = Guid.Empty;
    }
    
    public void UpdateUserInfo(string username, string email, string phone)
    {
        CurrentUsername = username;
        Email = email;
        Phone = phone;
    }
}