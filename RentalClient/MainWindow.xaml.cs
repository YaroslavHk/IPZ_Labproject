using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using RentalClient.UserControls;
using RentalClient.Interface;

namespace RentalClient;


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

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
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
                MessageBox.Show(
                    $"Failed to parse config.json. Make sure the JSON format is correct.\n\nError: {ex.Message}",
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
        TestApiFullCycleAsync();

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
    
    public async Task TestApiFullCycleAsync()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri(ServerUrl);
        string testSuffix = DateTime.Now.Ticks.ToString()[^6..];
        string testUsername = $"User_{testSuffix}";
        string testEmail = $"user_{testSuffix}@test.com";
        string testPassword = "SuperPassword123!";

        Console.WriteLine("=== STEP 1: REGISTRATION ===");
        var registerData = new RegisterRequest(testEmail, testUsername, "+380000000000", testPassword);
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerData);

        if (registerResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Success] User '{testUsername}' successfully registered!");
        }
        else
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[Error] Registration failed: {registerResponse.StatusCode}\nDetails: {error}");
            return;
        }


        Console.WriteLine("\n=== STEP 2: AUTHORIZATION ===");
        var loginData = new AuthRequest(testUsername, testPassword);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginData);

        if (!loginResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"[Error] Invalid login. Status: {loginResponse.StatusCode}");
            return;
        }

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        var token = authResult?.Token;
        Console.WriteLine($"[Success] Token received: {token?[..20]}...");


        Console.WriteLine("\n=== STEP 3: AUTHORIZATION SETUP ===");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        Console.WriteLine("Token successfully added to HttpClient headers.");


        Console.WriteLine("\n=== STEP 4: ADDING A NEW RENTAL ===");
        var newRental = new RentalRequest(
            Title: $"Cozy Apartment in Center {testSuffix}",
            Description: "A very nice place to live with great view.",
            Price: 15000,
            City: "Kyiv",
            Address: "Khreshchatyk St.",
            Type: "Apartment",
            LivingSpace: 45.5f
        );

        var createResponse = await client.PostAsJsonAsync("/api/rentals", newRental);

        if (createResponse.IsSuccessStatusCode)
        {
            var createdResult = await createResponse.Content.ReadFromJsonAsync<RentalPostResponse>();
            Console.WriteLine($"[Success] Rental created with ID: {createdResult?.Id}");
        }
        else
        {
            var error = await createResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"[Error] Failed to create rental: {createResponse.StatusCode}\nDetails: {error}");
        }


        Console.WriteLine("\n=== STEP 5: FETCHING RENTALS LIST ===");
        for (var i = 1; i < 5; i++)
        {
            var fetchResponse = await client.GetAsync($"/api/rentals?page={i}&pageSize=10");

            if (fetchResponse.IsSuccessStatusCode)
            {
                var rentals = await fetchResponse.Content.ReadFromJsonAsync<List<ShortRentalResponse>>();

                Console.WriteLine($"[Success] Rentals found: {rentals?.Count}");
                foreach (var rental in rentals ?? [])
                {
                    Console.WriteLine($" > {rental.Title} | City: {rental.City} | Price: {rental.Price}");
                }
            }
            else
            {
                var error = await fetchResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[Error] Failed to fetch list: {fetchResponse.StatusCode}\nDetails: {error}");
            }
        }
    }
    

private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        using HttpClient client = new HttpClient();
        Console.WriteLine(ServerUrl);
        if (ServerUrl != null)
        {
            client.BaseAddress = new Uri(ServerUrl);
        }
        int testId = 8; 

        try
        {
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
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            MessageBox.Show($"Rental with ID {testId} does not exist in the database.", 
                "Debug: Not Found (404)", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Cannot connect to the server. Check ngrok and the server!\n\nDetails: {ex.Message}", 
                "Debug: Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

