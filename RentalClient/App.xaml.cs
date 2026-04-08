using System.Configuration;
using System.Data;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RentalClient;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    
    
}

class appConfig
{
    public string ServerUrl { get; set; } = "";
}

class Rental
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string City { get; set; } = "";
    public string Type { get; set; } = "";
    public float LivingSpace{get; set;}
    public string Description { get; set; } = "";
}

public record AuthRequest(string Login, string Password);
public record AuthResponse(string Token);
public record ShortRentalResponse(Guid Id, string Title, decimal Price, string City);



public record RegisterRequest(string Email, string UserName, string Phone, string Password);
public record RentalRequest(string Title, string Description, decimal Price, string City, string Address, string Type, float LivingSpace);
public record RentalPostResponse(Guid Id);
