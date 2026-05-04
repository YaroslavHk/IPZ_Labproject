using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls;

public partial class ListingView : UserControl
{
    public ListingView(ListingViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}