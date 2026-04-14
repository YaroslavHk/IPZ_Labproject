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

        
        private string _fullName = "Завантаження...";
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); } }

        private string _phoneNumber = "";
        public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }

        private string _email = "";
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        
        public ObservableCollection<ShortRentalResponse> UserAds { get; } = new ObservableCollection<ShortRentalResponse>();

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
                    if (value) _ = LoadAdsAsync("favorites");
                }
            }
        }
        
        public ICommand CreateListingCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand UpdateAdCommand { get; }
        public ICommand DeleteAdCommand { get; }

        public ProfileViewModel(INavigationService navigationService, IRentalApiService apiService)
        {
            _navigationService = navigationService;
            _apiService = apiService;

            CreateListingCommand = new RelayCommand(o => _navigationService.NavigateWorkplaceTo<CreateListingView>());
            EditProfileCommand = new RelayCommand(ExecuteEditProfile);
            
            UpdateAdCommand = new RelayCommand(ExecuteUpdateAd);
            DeleteAdCommand = new RelayCommand(ExecuteDeleteAd);

            _ = LoadProfileInfoAsync();
            _ = LoadAdsAsync("my_ads");
        }


        private async Task LoadProfileInfoAsync()
        {
            FullName = "Іван Іванов";
            PhoneNumber = "+38(099)1234567";
            Email = "ivanov@example.com";
        }

        private async Task LoadAdsAsync(string category)
        {
            UserAds.Clear();
        }

        private void ExecuteEditProfile(object parameter)
        {
            MessageBox.Show("Тут буде логіка редагування профілю.", "Редагування");
        }

        private void ExecuteUpdateAd(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                MessageBox.Show($"Тут буде логіка оновлення для: {ad.Title}");
            }
        }

        private void ExecuteDeleteAd(object parameter)
        {
            if (parameter is ShortRentalResponse ad)
            {
                var result = MessageBox.Show($"Ви дійсно хочете видалити оголошення '{ad.Title}'?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    UserAds.Remove(ad);
                    // await _apiService.DeleteAdAsync(ad.Id);
                }
            }
        }
    }
}