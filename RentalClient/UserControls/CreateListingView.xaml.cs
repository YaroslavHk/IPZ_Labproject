using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class CreateListingView : UserControl
    {
        public CreateListingView(CreateListingViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;
        }
    }
}