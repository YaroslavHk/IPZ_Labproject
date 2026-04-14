using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls;

public partial class MainWindowView : UserControl
{
    public MainWindowView(MainLayoutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}