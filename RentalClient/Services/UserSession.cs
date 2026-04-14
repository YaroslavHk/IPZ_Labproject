namespace RentalClient.Services;

public interface IUserSession
{
    string Token { get; }
    bool IsAuthenticated { get; }

    string CurrentUsername { get; }    
    void Login(string token, string username);
    void Logout();
}

public class UserSession : IUserSession
{
    public string Token { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);
    public string CurrentUsername { get; private set; }
    
    public void Login(string token, string username)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Токен не может быть пустым.");

        Token = token;
        CurrentUsername = username;
    }

    public void Logout()
    {
        Token = null;
        CurrentUsername = null;
    }
}