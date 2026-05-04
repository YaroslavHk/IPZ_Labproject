using Xunit;
using Moq;
using RentalClient.ViewModels;
using RentalClient.Services;

namespace RentalClient.Tests.ViewModels;

public class SignUpViewModelTest
{
    [Theory]
    [InlineData("ValidPass123", "ValidPass123", true)]
    [InlineData("ValidPass!", "ValidPass!", true)]
    [InlineData("short1!", "short1!", false)]
    [InlineData("NoNumbersOrSymbols", "NoNumbersOrSymbols", false)]
    [InlineData("ValidPass123", "DifferentPass123", false)]
    public void ValidateForm_PasswordValidation_SetsFormValidityCorrectly(
        string password, string confirmPassword, bool expectedIsValid)
    {
        var mockNav = new Mock<INavigationService>();
        var mockApi = new Mock<IRentalApiService>();
        var mockDialog = new Mock<IDialogService>();

        var viewModel = new SignUpViewModel(mockNav.Object, mockApi.Object, mockDialog.Object)
        {
            Email = "test@example.com",
            UserName = "TestUser",
            Phone = "123456789"
        };

        viewModel.CurrentPassword = password;
        viewModel.CurrentConfirmPassword = confirmPassword;

        bool canExecute = viewModel.RegisterCommand.CanExecute(null);
        Assert.Equal(expectedIsValid, canExecute);
    }
}