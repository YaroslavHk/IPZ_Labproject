using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class CreateListingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;
        
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

        // Состояние радио-кнопок
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
                ((RelayCommand)CreateListingCommand).RaiseCanExecuteChanged();
            }
        }
        
        public ICommand CreateListingCommand { get; }

        public CreateListingViewModel(INavigationService navigationService, IRentalApiService apiService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            
            CreateListingCommand = new RelayCommand(ExecuteCreateListing, CanExecuteCreateListing);
        }

        private async void ExecuteCreateListing(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Price) || 
                string.IsNullOrWhiteSpace(LivingSpace) || string.IsNullOrWhiteSpace(City) || 
                string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(RoomQuantity))
            {
                MessageBox.Show("Будь ласка, заповніть всі обов'язкові поля.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(Price, out decimal parsedPrice) || parsedPrice < 0)
            {
                MessageBox.Show("Введіть коректну ціну (число більше нуля).", "Помилка формату", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!int.TryParse(RoomQuantity, out int parsedRooms) || parsedRooms < 1)
            {
                MessageBox.Show("Кількість кімнат повинна бути цілим числом (1 або більше).", "Помилка формату", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!float.TryParse(LivingSpace, out float parsedSpace) || parsedSpace < 0)
            {
                MessageBox.Show("Введіть коректну площу (число більше нуля).", "Помилка формату", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string type = IsApartment ? "Квартира" : "Будинок";

            IsLoading = true; 

            try
            {
                var request = new RentalRequest(Title, Description, parsedPrice, City, Address, type, parsedSpace, parsedRooms);
                
                bool isSuccess = await _apiService.CreateListingAsync(request);

                if (isSuccess)
                {
                    _navigationService.NavigateWorkplaceTo<SearchView>();
                }
                else
                {
                    MessageBox.Show("Сервер відхилив створення оголошення", "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteCreateListing(object parameter) => !IsLoading;
    }
}