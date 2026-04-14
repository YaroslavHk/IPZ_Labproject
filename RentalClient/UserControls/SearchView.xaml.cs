using System.Windows.Controls;
using RentalClient.ViewModels;

namespace RentalClient.UserControls
{
    public partial class SearchView : UserControl
    {
        public SearchView(SearchViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;
        }
    }
}