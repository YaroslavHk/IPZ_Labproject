using System.Windows.Controls;
using System.Windows.Input;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class ProfileView : UserControl, IMainViewInterface
{
    public event EventHandler<WorkplaceType> NavigationWorkplaceRequested;
    
    public ProfileView()
    {
        InitializeComponent();
    }

    private void CreateListing_Button(object sender, MouseButtonEventArgs e)
    {
        NavigationWorkplaceRequested?.Invoke(this, WorkplaceType.CreateListing);
    }
}