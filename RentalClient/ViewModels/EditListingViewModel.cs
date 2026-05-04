using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class EditListingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;
        private readonly IListingStateService _listingState;
        private readonly IDialogService _dialogService;
        
        private Guid _currentListingId;

        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _price;
        public string Price { get => _price; set { _price = value; OnPropertyChanged(); } }

        private string _roomQuantity;
        public string RoomQuantity { get => _roomQuantity; set { _roomQuantity = value; OnPropertyChanged(); } }

        private string _livingSpace;
        public string LivingSpace { get => _livingSpace; set { _livingSpace = value; OnPropertyChanged(); } }

        private string _city;
        public string City { get => _city; set { _city = value; OnPropertyChanged(); } }

        private string _address;
        public string Address { get => _address; set { _address = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private bool _isApartment = true;
        public bool IsApartment { get => _isApartment; set { _isApartment = value; OnPropertyChanged(); } }

        private bool _isHouse;
        public bool IsHouse { get => _isHouse; set { _isHouse = value; OnPropertyChanged(); } }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)UpdateListingCommand).RaiseCanExecuteChanged();
            }
        }
        
        public ICommand UpdateListingCommand { get; }
        public ICommand CancelCommand { get; }

        public EditListingViewModel(INavigationService navigationService, IRentalApiService apiService, IListingStateService listingState, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _listingState = listingState;
            _dialogService = dialogService;
            
            UpdateListingCommand = new RelayCommand(ExecuteUpdateListing, CanExecuteUpdateListing);
            CancelCommand = new RelayCommand(o => _navigationService.NavigateWorkplaceTo<ProfileView>());

            if (_listingState.EditingListingId != Guid.Empty)
            {
                _currentListingId = _listingState.EditingListingId;
                _ = LoadListingDataAsync();
            }
        }

        private async Task LoadListingDataAsync()
        {
            IsLoading = true;
            try
            {
                var rental = await _apiService.GetRentalByIdAsync(_currentListingId);
                
                if (rental != null)
                {
                    Title = rental.Title;
                    Price = rental.Price.ToString();
                    RoomQuantity = rental.RoomCount.ToString();
                    LivingSpace = rental.LivingSpace.ToString();
                    City = rental.City;
                    Address = rental.Address;
                    Description = rental.Description;

                    if (rental.Type == "Будинок")
                    {
                        IsHouse = true;
                        IsApartment = false;
                    }
                    else
                    {
                        IsApartment = true;
                        IsHouse = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Не вдалося завантажити дані для редагування: {ex.Message}");
                _navigationService.NavigateWorkplaceTo<ProfileView>();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ExecuteUpdateListing(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Price) || 
                string.IsNullOrWhiteSpace(LivingSpace) || string.IsNullOrWhiteSpace(City) || 
                string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(RoomQuantity))
            {
                _dialogService.ShowWarning("Будь ласка, заповніть всі обов'язкові поля.");
                return;
            }

            if (!decimal.TryParse(Price, out decimal parsedPrice) || parsedPrice < 0) return;
            if (!int.TryParse(RoomQuantity, out int parsedRooms) || parsedRooms < 1) return;
            if (!float.TryParse(LivingSpace, out float parsedSpace) || parsedSpace < 0) return;

            string type = IsApartment ? "Квартира" : "Будинок";
            IsLoading = true;

            try
            {
                var request = new RentalRequest(Title, Description, parsedPrice, City, Address, type, parsedSpace, parsedRooms);
                
                bool isSuccess = await _apiService.UpdateListingAsync(_currentListingId, request);

                if (isSuccess)
                {
                    _dialogService.ShowInfo("Оголошення успішно оновлено!");
                    _listingState.EditingListingId = Guid.Empty;
                    _navigationService.NavigateWorkplaceTo<ProfileView>();
                }
                else
                {
                    _dialogService.ShowError("Сервер відхилив оновлення.");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Помилка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteUpdateListing(object parameter) => !IsLoading;
    }
}