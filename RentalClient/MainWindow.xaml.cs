using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using RentalClient.UserControls;
using RentalClient.Interface;

namespace RentalClient; // Replace with your actual namespace if different


public partial class MainWindow : Window
{
    private readonly string? ServerUrl;
    public MainWindow()
    {
        
        InitializeComponent();
        NavigateTo(PageType.Main);
        if (File.Exists("config.json"))
        {
            try 
            {
                string json = File.ReadAllText("config.json");

                // 1. Tell the serializer to ignore uppercase/lowercase differences
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                };

                // 2. Deserialize using the options
                var config = JsonSerializer.Deserialize<appConfig>(json, options);

                if (config != null && !string.IsNullOrWhiteSpace(config.ServerUrl))
                {
                    ServerUrl = config.ServerUrl;
                }
                else 
                {
                    MessageBox.Show("Config file found, but ServerUrl is missing or empty inside the JSON.", 
                        "Config Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (JsonException ex)
            {
                // 3. Catch JSON syntax errors (e.g., missing quotes, trailing commas)
                MessageBox.Show($"Failed to parse config.json. Make sure the JSON format is correct.\n\nError: {ex.Message}", 
                    "JSON Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else 
        {
            MessageBox.Show("config.json file not found! Make sure 'Copy to Output Directory' is set to 'Copy always'.", 
                "Missing File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        OnNavigationPageRequested(this, PageType.Main);
        this.Loaded += MainWindow_Loaded;
        
    }

    private void NavigateTo(PageType page)
    {
        UserControl newView = page switch
        {
            PageType.Main => new MainWindowView(),
            PageType.SignIn => new SignInView(),
            PageType.SignUp => new SignUpView(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (newView is IViewInterface navigableView)
        {
            navigableView.NavigationPageRequested += OnNavigationPageRequested;
        }
        
        MainHost.Content = newView;
    }
    
    private void OnNavigationPageRequested(object? sender, PageType targetPage)
    {
        NavigateTo(targetPage);
    }
    















    
    
    
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        using HttpClient client = new HttpClient();
        Console.WriteLine(ServerUrl);
        if (ServerUrl != null)
        {
            client.BaseAddress = new Uri(ServerUrl);
        }
        // The ID we are trying to fetch
        int testId = 8; 

        try
        {
            // Send the GET request to the server
            var rentals = await client.GetFromJsonAsync<List<Rental>>($"/api/rentals");

            if (rentals != null)
            {
                foreach (var rental in rentals)
                {
                    MessageBox.Show(
                        $"ID: {rental.Id}\n" +
                        $"Title: {rental.Title}\n" +
                        $"City: {rental.City}\n" +
                        $"Price: {rental.Price} UAH\n"+ 
                        $"LivingSpace: {rental.LivingSpace}\n"+
                        $"Type: {rental.Type}\n"+
                        $"Description: {rental.Description}",
                        "Debug: Success (200 OK)", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Show the result in a popup window
                
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Specifically catching the 404 Not Found error
            MessageBox.Show($"Rental with ID {testId} does not exist in the database.", 
                "Debug: Not Found (404)", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            // Catching any connection drops or ngrok issues
            MessageBox.Show($"Cannot connect to the server. Check ngrok and the server!\n\nDetails: {ex.Message}", 
                "Debug: Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

