using System.Windows;

namespace RentalClient.Services;

public interface IDialogService
{
    void ShowInfo(string message, string title = "Інформація");
    void ShowError(string message, string title = "Помилка");
    void ShowWarning(string message, string title = "Увага");
    bool ShowConfirmation(string message, string title = "Підтвердження");
}

public class DialogHelper : IDialogService
{
    public void ShowInfo(string message, string title = "Інформація")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Помилка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Увага")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "Підтвердження")
    {
        MessageBoxResult result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}