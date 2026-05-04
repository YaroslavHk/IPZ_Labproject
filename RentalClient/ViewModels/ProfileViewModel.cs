using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class ProfileViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;
        private readonly IUserSession _userSession;
        private readonly IListingStateService _listingState;
        private readonly IDialogService _dialogService;
        
        private string _fullName = "Завантаження...";
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _phoneNumber = "";
        public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }

        private string _email = "";
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        
        public ObservableCollection<ShortRentalResponse> UserAds { get; } = new ObservableCollection<ShortRentalResponse>();

        public bool IsFavoritesViewActive => IsFavoritesSelected;

        private bool _isMyAdsSelected = true;
        public bool IsMyAdsSelected
        {
            get => _isMyAdsSelected;
            set
            {
                if (_isMyAdsSelected != value)
                {
                    _isMyAdsSelected = value;
                    OnPropertyChanged();
                    if (value) _ = LoadAdsAsync("my_ads");
                }
            }
        }

        private bool _isFavoritesSelected;
        public bool IsFavoritesSelected
        {
            get => _isFavoritesSelected;
            set
            {
                if (_isFavoritesSelected != value)
                {
                    _isFavoritesSelected = value;
                    OnPropertyChanged();
                    
                    OnPropertyChanged(nameof(IsFavoritesViewActive)); 
                    
                    if (value) _ = LoadAdsAsync("favorites");
                }
            }
        }
        
        private bool _isEditing;
        public bool IsEditing 
        { 
            get => _isEditing; 
            set { _isEditing = value; OnPropertyChanged(); } 
        }
        
        private bool _isLoading;
        public bool IsLoading 
        { 
            get => _isLoading; 
            set { _isLoading = value; OnPropertyChanged(); } 
        }
        
        public ICommand CreateListingCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand LogOutCommand { get; }
        public ICommand UpdateAdCommand { get; }
        public ICommand DeleteAdCommand { get; }
        public ICommand OpenListingCommand { get; }
        public ICommand HideAdCommand { get; }
        public ICommand RemoveFromFavoritesCommand { get; }
        

        public ProfileViewModel(INavigationService navigationService, IRentalApiService apiService, IUserSession userSession, IListingStateService listingState, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _userSession = userSession;
            _listingState = listingState;
            _dialogService = dialogService;

            CreateListingCommand = new RelayCommand(o => _navigationService.NavigateWorkplaceTo<CreateListingView>());
            EditProfileCommand = new RelayCommand(ExecuteEditProfile);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
            LogOutCommand = new RelayCommand(ExecuteLogOut);
            DeleteAdCommand = new  RelayCommand(ExecuteDeleteAd);
            UpdateAdCommand = new RelayCommand(ExecuteUpdateAd);
            HideAdCommand = new RelayCommand(ExecuteHideAd);
            RemoveFromFavoritesCommand = new RelayCommand(ExecuteRemoveFromFavorites);

            OpenListingCommand = new RelayCommand(ExecuteOpenListing);
            
            _ = LoadProfileInfoAsync();
            _ = LoadAdsAsync("my_ads");
        }


        private async Task LoadProfileInfoAsync()
        {
            FullName = _userSession.CurrentUsername ?? "Ім'я не вказано";
            PhoneNumber = _userSession.Phone ?? "Телефон не вказано";
            Email = _userSession.Email ?? "Email не вказано";
            
            await Task.CompletedTask;
        }
        
        private void ExecuteLogOut(object parameter)
        {
            _userSession.Logout();
            _navigationService.NavigateWorkplaceTo<SearchView>();
        }

        private void ExecuteOpenListing(object parameter)
        {
            if (parameter is Guid listingId)
            {
                _listingState.SelectedListingId = listingId;
                _navigationService.NavigateWorkplaceTo<ListingView>();
            }
        }
        
        private async Task LoadAdsAsync(string category)
        {
            IsLoading = true;
            UserAds.Clear();

            try
            {
                if (category == "my_ads")
                {
                    var myAds = await _apiService.GetMyRentalsAsync();
                    foreach (var ad in myAds)
                    {
                        UserAds.Add(ad);
                    }
                }
                else if (category == "favorites")
                {
                    // РАЦИОНАЛЬНО: Убрана заглушка, добавлен реальный запрос
                    var favoriteAds = await _apiService.GetFavoriteRentalsAsync();
                    foreach (var ad in favoriteAds)
                    {
                        UserAds.Add(ad);
                    }
                    
                    if (!favoriteAds.Any())
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Помилка завантаження оголошень: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void ExecuteEditProfile(object parameter)
        {
            if (!IsEditing)
            {
                IsEditing = true;
            }
            else
            {
                try
                {
                    var request = new UpdateProfileRequest(_userSession.UserId, FullName, Email, PhoneNumber);
                    bool success = await _apiService.UpdateProfileAsync(request);

                    if (success)
                    {
                        _userSession.UpdateUserInfo(FullName, Email, PhoneNumber);
                        IsEditing = false;
                        _dialogService.ShowInfo("Профіль успішно оновлено!");
                    }
                    else
                    {
                        _dialogService.ShowError("Помилка при оновленні.");
                    }
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Помилка мережі: {ex.Message}");
                }
            }
        }
        
        private void ExecuteCancelEdit(object parameter)
        {
            FullName = _userSession.CurrentUsername ?? "Ім'я не вказано";
            PhoneNumber = _userSession.Phone ?? "Телефон не вказано";
            Email = _userSession.Email ?? "Email не вказано";

            IsEditing = false;
        }
        
        private async void ExecuteDeleteAd(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                if (_dialogService.ShowConfirmation($"Ви дійсно хочете видалити оголошення '{ad.Title}'?"))
                {
                    IsLoading = true;
                    try
                    {
                        bool success = await _apiService.DeleteRentalAsync(ad.Id);

                        if (success)
                        {
                            UserAds.Remove(ad);
                            _dialogService.ShowInfo("Оголошення успішно видалено.");
                        }
                        else
                        {
                            _dialogService.ShowError("Не вдалося видалити оголошення на сервері. Можливо, у вас недостатньо прав.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowError($"Помилка мережі при видаленні: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false; 
                    }
                }
            }
        }
        
        private void ExecuteUpdateAd(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                _listingState.EditingListingId = ad.Id;
                _navigationService.NavigateWorkplaceTo<EditListingView>();
            }
        }
        
        private async void ExecuteHideAd(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                IsLoading = true;
                try
                {
                    bool success = await _apiService.HideRentalAsync(ad.Id);

                    if (success)
                    {
                        _dialogService.ShowInfo("Видимість голошення успішно оновлено.");
                    }
                    else
                    {
                        _dialogService.ShowError("Не вдалося оновити оголошення. Можливо, сталася помилка на сервері.");
                    }
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Помилка мережі: {ex.Message}");
                    
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        private async void ExecuteRemoveFromFavorites(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                if (_dialogService.ShowConfirmation($"Ви дійсно хочете прибрати '{ad.Title}' з обраного?"))
                {
                    IsLoading = true;
                    try
                    {
                        bool success = await _apiService.RemoveFromFavoritesAsync(ad.Id);

                        if (success)
                        {
                            UserAds.Remove(ad); 
                            _dialogService.ShowInfo("Оголошення успішно прибрано з обраного.");
                            
                        }
                        else
                        {
                            _dialogService.ShowError("Не вдалося прибрати оголошення. Можливо, сталася помилка на сервері.");
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowError($"Помилка мережі: {ex.Message}");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }
}