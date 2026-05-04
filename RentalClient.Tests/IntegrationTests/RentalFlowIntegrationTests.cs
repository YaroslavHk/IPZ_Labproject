using Xunit;
using Moq;
using RentalClient.ViewModels;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.Tests.IntegrationTests;

public class RentalFlowIntegrationTests
{
    // Тест ланцюга: Реєстрація -> Авторизація -> Перехід на головну[cite: 1, 4]
    [Fact]
    public async Task RegistrationToLoginFlow_SuccessfulSequence()
    {
        // Arrange
        var mockApi = new Mock<IRentalApiService>();
        var mockNav = new Mock<INavigationService>();
        var mockDialog = new Mock<IDialogService>();

        // Імітуємо успішну реєстрацію та наступний логін[cite: 4, 6]
        mockApi.Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>())).ReturnsAsync(true);
        mockApi.Setup(a => a.LoginAsync(It.IsAny<AuthRequest>())).ReturnsAsync(true);

        var signUpVm = new SignUpViewModel(mockNav.Object, mockApi.Object, mockDialog.Object)
        {
            Email = "newuser@khnure.ua",
            UserName = "IntegrationTester",
            CurrentPassword = "Password123!",
            CurrentConfirmPassword = "Password123!"
        };

        // Act
        signUpVm.RegisterCommand.Execute(null);

        // Assert: Перевіряємо, що ланцюг подій призвів до навігації на головний екран[cite: 4]
        mockNav.Verify(n => n.NavigateRootTo<MainWindowView>(), Times.AtLeastOnce());
    }
}