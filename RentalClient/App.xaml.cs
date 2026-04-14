using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RentalClient.Services;
using RentalClient.UserControls;
using RentalClient.ViewModels;

namespace RentalClient;

public partial class App : Application
{ 
    public IServiceProvider ServiceProvider { get; private set; }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
            string apiUrl = ConfigLoader.LoadServerUrl();
        
            var services = new ServiceCollection();
            ConfigureServices(services, apiUrl);
            ServiceProvider = services.BuildServiceProvider();
        
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
            
            var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
            
            navigationService.NavigateRootTo<MainWindowView>();
    }
    
    private void ConfigureServices(IServiceCollection services, string apiUrl)
    {
        services.AddTransient<AuthHeaderHandler>();
        
        services.AddHttpClient<IRentalApiService, RentalApiService>(client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();
        
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUserSession, UserSession>();
        
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainLayoutViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<CreateListingViewModel>();
        services.AddTransient<SignInViewModel>();
        services.AddTransient<SignUpViewModel>();
        
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<SearchView>();
        services.AddTransient<CreateListingView>();
        services.AddTransient<SignInView>();
        services.AddTransient<SignUpView>();
    }
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
public record RegisterRequest(string Email, string UserName, string Phone, string Password);

public record ShortRentalResponse(Guid Id, string Title, decimal Price, string City, string ImageUrl);
public record RentalRequest(string Title, string Description, decimal Price, string City, string Address, string Type, float LivingSpace, int QuantityRooms);
public record RentalPostResponse(Guid Id);