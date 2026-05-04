using System.Windows;
using RentalClient.ViewModels;

namespace RentalClient;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}