using System.Windows;
using System.Windows.Controls;
using RentalClient.Interface;

namespace RentalClient.UserControls;

public partial class SignInView : UserControl, IViewInterface
{
    public event EventHandler<PageType> NavigationPageRequested;
    public SignInView()
    {
        InitializeComponent();
    }

    private void SignUpButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationPageRequested?.Invoke(this, PageType.SignUp);
    }
    
    private void BackToMainWindowButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationPageRequested?.Invoke(this, PageType.Main);
    }
}