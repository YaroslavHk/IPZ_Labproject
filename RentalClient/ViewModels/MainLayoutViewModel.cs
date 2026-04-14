using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels;

public class MainLayoutViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IUserSession _userSession;

    // Свойство для внутреннего макета
    public object CurrentWorkplaceView => _navigationService.CurrentWorkplaceView;

    public ICommand ProfileCommand { get; }
    public ICommand HomeCommand { get; }

    public MainLayoutViewModel(INavigationService navigationService, IUserSession userSession)
    {
        _navigationService = navigationService;
        _userSession = userSession;

        // Подписка на локальную навигацию
        _navigationService.WorkplaceViewChanged += () => OnPropertyChanged(nameof(CurrentWorkplaceView));

        ProfileCommand = new RelayCommand(ExecuteProfile);
        HomeCommand = new RelayCommand(ExecuteHome);

        // Стартовая загрузка
        _navigationService.NavigateWorkplaceTo<SearchView>();
    }

    private void ExecuteProfile(object parameter)
    {
        if (_userSession.IsAuthenticated)
        {
            _navigationService.NavigateWorkplaceTo<ProfileView>();
        }
        else
        {
            _navigationService.NavigateRootTo<SignInView>();
        }
    }

    private void ExecuteHome(object parameter)
    {
        _navigationService.NavigateWorkplaceTo<SearchView>();
    }
}