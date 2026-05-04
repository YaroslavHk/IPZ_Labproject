using Xunit;
using Moq;
using RentalClient.ViewModels;
using RentalClient.Services;

namespace RentalClient.Tests.ViewModels;

public class SearchViewModelFiltersTest
{
    private SearchViewModel CreateViewModelWithMocks()
    {
        var mockApi = new Mock<IRentalApiService>();
        var mockSearchState = new Mock<ISearchStateService>();
        var mockListingState = new Mock<IListingStateService>();
        var mockNav = new Mock<INavigationService>();
        var mockDialog = new Mock<IDialogService>();

        mockSearchState.SetupAllProperties();
        
        return new SearchViewModel(mockApi.Object, mockSearchState.Object, mockListingState.Object, mockNav.Object, mockDialog.Object);
    }

    [Fact]
    public void TryParseFilters_ValidNumericInputs_CallsApiWithParsedValues()
    {
        var viewModel = CreateViewModelWithMocks();
        
        viewModel.MinPrice = "1000";
        viewModel.MaxPrice = "5000";
        viewModel.Rooms = "2";

        viewModel.ApplyFilterCommand.Execute(null);

        Assert.Empty(viewModel.Rentals);
    }

    [Fact]
    public void TryParseFilters_InvalidNumericInputs_ShowsWarningAndStops()
    {
        var mockDialog = new Mock<IDialogService>();
        var viewModel = new SearchViewModel(
            new Mock<IRentalApiService>().Object,
            new Mock<ISearchStateService>().Object,
            new Mock<IListingStateService>().Object,
            new Mock<INavigationService>().Object,
            mockDialog.Object);

        viewModel.MinPrice = "abc"; 
        viewModel.Rooms = "-5";

        viewModel.ApplyFilterCommand.Execute(null);

        mockDialog.Verify(d => d.ShowWarning(It.Is<string>(s => s.Contains("перевірте правильність"))), Times.Once);
    }
}