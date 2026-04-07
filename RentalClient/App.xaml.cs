using System.Configuration;
using System.Data;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
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

