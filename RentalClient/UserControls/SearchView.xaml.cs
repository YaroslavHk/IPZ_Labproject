using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class SearchView : UserControl, IMainViewInterface
{
    public event EventHandler<WorkplaceType> NavigationWorkplaceRequested;
    public SearchView()
    {
        InitializeComponent();
    }
    
}