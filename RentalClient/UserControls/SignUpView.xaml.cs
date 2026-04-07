using System.Windows;
using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class SignUpView : UserControl, IViewInterface
{
    public event EventHandler<PageType> NavigationPageRequested;
    public SignUpView()
    {
        InitializeComponent();
    }

    private void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationPageRequested?.Invoke(this, PageType.SignIn);
    }
    
    private void BackToMainWindowButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationPageRequested?.Invoke(this, PageType.Main);
    }
}