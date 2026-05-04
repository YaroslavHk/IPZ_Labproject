using System.Windows;
using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class SignUpView : UserControl
    {
        public SignUpView(SignUpViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SignUpViewModel viewModel)
            {
                viewModel.CurrentPassword = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SignUpViewModel viewModel)
            {
                viewModel.CurrentConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}