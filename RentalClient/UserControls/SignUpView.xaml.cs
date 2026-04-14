using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class SignUpView : UserControl
    {
        public SignUpView(SignUpViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;
        }
    }
}