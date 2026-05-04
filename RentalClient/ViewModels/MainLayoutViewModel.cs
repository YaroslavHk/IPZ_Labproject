using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels;

public class MainLayoutViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IUserSession _userSession;
    
    public ISearchStateService SearchState { get; }
    public object CurrentWorkplaceView => _navigationService.CurrentWorkplaceView;

    public ICommand ProfileCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand PerformGlobalSearchCommand { get; }
    
    public MainLayoutViewModel(INavigationService navigationService, IUserSession userSession, ISearchStateService searchState)
    {
        _navigationService = navigationService;
        _userSession = userSession;
        SearchState = searchState;

        _navigationService.WorkplaceViewChanged += () => OnPropertyChanged(nameof(CurrentWorkplaceView));

        ProfileCommand = new RelayCommand(ExecuteProfile);
        HomeCommand = new RelayCommand(ExecuteHome);

        PerformGlobalSearchCommand = new RelayCommand(ExecuteGlobalSearch);
        
        _navigationService.NavigateWorkplaceTo<SearchView>();
    }

    private void ExecuteGlobalSearch(object parameter)
    {
        if (_navigationService.CurrentWorkplaceView is SearchView)
        {
            SearchState.TriggerSearch();
        }
        else
        {
            _navigationService.NavigateWorkplaceTo<SearchView>();
        }
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