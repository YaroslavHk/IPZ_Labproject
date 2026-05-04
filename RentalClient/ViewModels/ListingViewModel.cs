using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class ListingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;
        private readonly IListingStateService _listingState;
        private readonly IDialogService _dialogService;

        private string _priceString = "Завантаження...";
        public string PriceString { get => _priceString; set { _priceString = value; OnPropertyChanged(); } }

        private string _fullAddress;
        public string FullAddress { get => _fullAddress; set { _fullAddress = value; OnPropertyChanged(); } }

        private string _specs;
        public string Specs { get => _specs; set { _specs = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _mainImageUrl;
        public string MainImageUrl { get => _mainImageUrl; set { _mainImageUrl = value; OnPropertyChanged(); } }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
        
        public ICommand GoBackCommand { get; }
        public ICommand ContactOwnerCommand { get; }
        public ICommand AddToFavoritesCommand { get; }
        

        public ListingViewModel(INavigationService navigationService, IRentalApiService apiService, IListingStateService listingState, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _listingState = listingState;
            _dialogService = dialogService;

            GoBackCommand = new RelayCommand(o => _navigationService.NavigateWorkplaceTo<SearchView>());
            ContactOwnerCommand = new RelayCommand(o => MessageBox.Show("Функція зв'язку в розробці", "Інфо"));
            AddToFavoritesCommand = new RelayCommand(ExecuteAddToFavorites);
            
            if (_listingState.SelectedListingId != Guid.Empty)
            {
                _ = LoadListingAsync(_listingState.SelectedListingId);
            }
        }
        
        public async Task LoadListingAsync(Guid listingId)
        {
            IsLoading = true; 
            
            try
            {
                var rental = await _apiService.GetRentalByIdAsync(listingId);

                if (rental != null)
                {
                    PriceString = $"{rental.Price:N0} ГРН / МІСЯЦЬ";
                    
                    string cityStr = rental.City?.ToUpper() ?? "НЕВІДОМЕ МІСТО";
                    FullAddress = $"АДРЕСА: М. {cityStr}, {rental.Address}";
                    Specs = $"КІМНАТ: {rental.RoomCount} | ПЛОЩА: {rental.LivingSpace} КВ.М.";
                    Description = $"ОПИС: {rental.Description}";
                    
                    if (rental.ImageUrl != null && rental.ImageUrl.Any())
                    {
                        MainImageUrl = rental.ImageUrl.First();
                    }
                    else
                    {
                        MainImageUrl = "placeholder_image_url_here";
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не вдалося завантажити дані: {ex.Message}");
                _navigationService.NavigateWorkplaceTo<SearchView>();
            }
            finally
            {
                IsLoading = false; 
            }
        }
        
        private async void ExecuteAddToFavorites(object parameter)
        {
            if (_listingState.SelectedListingId == Guid.Empty) return;

            IsLoading = true;
            try
            {
                bool success = await _apiService.AddToFavoritesAsync(_listingState.SelectedListingId);

                if (success)
                {
                    _dialogService.ShowInfo("Оголошення успішно додано до обраного!");
                }
                else
                {
                    _dialogService.ShowWarning("Не вдалося додати до обраного. Можливо, це оголошення вже збережено.");
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