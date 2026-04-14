using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class SignInView : UserControl
    {
        public SignInView(SignInViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;
        }
    }
}