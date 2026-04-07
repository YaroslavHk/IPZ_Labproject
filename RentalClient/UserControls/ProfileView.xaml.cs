using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class ProfileView : UserControl, IMainViewInterface
{
    public event EventHandler<WorkplaceType> NavigationWorkplaceRequested;
    
    public ProfileView()
    {
        InitializeComponent();
    }
}