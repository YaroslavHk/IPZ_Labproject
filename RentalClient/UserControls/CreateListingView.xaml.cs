using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class CreateListingView : UserControl, IMainViewInterface
{
    public event EventHandler<WorkplaceType> NavigationWorkplaceRequested;
    
    public CreateListingView()
    {
        InitializeComponent();
    }
}