using Xunit;
using Moq;
using RentalClient.ViewModels;
using RentalClient.Services;

namespace RentalClient.Tests.ViewModels;

public class CreateListingViewModelTest
{
    [Fact]
    public void ExecuteCreateListing_ApiReturnsNull_ShowsErrorMessage()
    {
        var mockNavService = new Mock<INavigationService>(); 
        var mockApiService = new Mock<IRentalApiService>();
        var mockDialogService = new Mock<IDialogService>();
        
        mockApiService
            .Setup(api => api.CreateListingAsync(It.IsAny<RentalRequest>()))
            .ReturnsAsync((Guid?)null);

        var viewModel = new CreateListingViewModel(mockNavService.Object, mockApiService.Object, mockDialogService.Object)
        {
            Title = "Будинок біля озера",
            Price = "1500",
            LivingSpace = "50",
            City = "Київ",
            Address = "Вулиця Тестова 1",
            RoomQuantity = "2"
        };

        viewModel.CreateListingCommand.Execute(null);

        mockDialogService.Verify(
            d => d.ShowError(It.IsAny<string>(), It.IsAny<string>()), 
            Times.Once); 
    }
}