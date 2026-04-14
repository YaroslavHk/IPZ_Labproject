using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class ProfileView : UserControl
    {
        public ProfileView(ProfileViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}