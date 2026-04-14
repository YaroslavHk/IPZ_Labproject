using RentalClient.Services;
using RentalClient.Core;
using RentalClient.UserControls;

namespace RentalClient.ViewModels;

public class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    // Свойство, к которому привязан глобальный ContentControl
    public object CurrentRootView => _navigationService.CurrentRootView;

    public ShellViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        // Подписываемся на события навигатора и уведомляем UI об изменении свойства
        _navigationService.RootViewChanged += () => OnPropertyChanged(nameof(CurrentRootView));
    }
}