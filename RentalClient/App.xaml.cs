using System;
using System.Net;
using System.Net.Http;
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
        ServicePointManager.DefaultConnectionLimit = 10;
        
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
        // HTTP & API Services
        services.AddTransient<AuthHeaderHandler>();
        
        services.AddHttpClient<IRentalApiService, RentalApiService>(client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();
        
        // State & Core Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IUserSession, UserSession>();
        services.AddSingleton<ISearchStateService, SearchStateService>();
        services.AddSingleton<IListingStateService, ListingStateService>();
        services.AddSingleton<IDialogService, DialogHelper>();
        
        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainLayoutViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<SearchViewModel>();
        services.AddTransient<CreateListingViewModel>();
        services.AddTransient<SignInViewModel>();
        services.AddTransient<SignUpViewModel>();
        services.AddTransient<ListingViewModel>();
        services.AddTransient<EditListingViewModel>();
        
        // Views
        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<SearchView>();
        services.AddTransient<CreateListingView>();
        services.AddTransient<SignInView>();
        services.AddTransient<SignUpView>();
        services.AddTransient<ListingView>();
        services.AddTransient<EditListingView>();
    }
}