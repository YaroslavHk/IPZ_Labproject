using System.Windows;
using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class MainWindowView : UserControl, IViewInterface, IMainViewInterface
{
    public event EventHandler<PageType> NavigationPageRequested;
    public event EventHandler<WorkplaceType> NavigationWorkplaceRequested;

    public bool isLogin = true;
    public MainWindowView()
    {
        InitializeComponent();
        NavigateTo(WorkplaceType.Search);
        
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isLogin)
        {
            NavigationPageRequested?.Invoke(this, PageType.SignIn);
        }
        else
        {
            NavigateTo(WorkplaceType.Profile);
        }


    }

    private void NavigateTo(WorkplaceType workplace)
    {
        UserControl newWorkplace = workplace switch
        {
            WorkplaceType.Search => new SearchView(),
            WorkplaceType.Profile => new ProfileView(),
            WorkplaceType.CreateListing => new CreateListingView(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (newWorkplace is IMainViewInterface navigableWorkplace)
        {
            navigableWorkplace.NavigationWorkplaceRequested += OnNavigationWorkplaceRequested;
        }
        
        WorkplaceHost.Content = newWorkplace;
    }
    
    private void OnNavigationWorkplaceRequested(object? sender, WorkplaceType workplaceType)
    {
        NavigateTo(workplaceType);
    }
    
}