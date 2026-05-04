using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls;

public partial class EditListingView : UserControl
{
    public EditListingView(EditListingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}