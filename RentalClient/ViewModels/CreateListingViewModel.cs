using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;
using Microsoft.Win32;

namespace RentalClient.ViewModels
{
    public class CreateListingViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;
        private readonly IDialogService _dialogService;
        
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
                ((RelayCommand)CreateListingCommand).RaiseCanExecuteChanged();
            }
        }
        
        public ObservableCollection<string> SelectedPhotos { get; } = new ObservableCollection<string>();
        public ICommand CreateListingCommand { get; }
        public ICommand SelectPhotosCommand { get; }

        public CreateListingViewModel(INavigationService navigationService, IRentalApiService apiService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _dialogService = dialogService;
            
            CreateListingCommand = new RelayCommand(ExecuteCreateListing, CanExecuteCreateListing);
            SelectPhotosCommand = new RelayCommand(ExecuteSelectPhotos);
        }

        private async void ExecuteCreateListing(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Price) || 
                string.IsNullOrWhiteSpace(LivingSpace) || string.IsNullOrWhiteSpace(City) || 
                string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(RoomQuantity))
            {
                _dialogService.ShowWarning("Будь ласка, заповніть всі обов'язкові поля.");
                return;
            }

            if (!decimal.TryParse(Price, out decimal parsedPrice) || parsedPrice < 0)
            {
                _dialogService.ShowWarning("Введіть коректну ціну (число більше нуля).");
                return;
            }
            
            if (!int.TryParse(RoomQuantity, out int parsedRooms) || parsedRooms < 1)
            {
                _dialogService.ShowWarning("Кількість кімнат повинна бути цілим числом (1 або більше).");
                return;
            }

            if (!float.TryParse(LivingSpace, out float parsedSpace) || parsedSpace < 0)
            {
                _dialogService.ShowWarning("Введіть коректну площу (число більше нуля).");
                return;
            }

            string type = IsApartment ? "Квартира" : "Будинок";
            IsLoading = true;

            try
            {
                var request = new RentalRequest(Title, Description, parsedPrice, City, Address, type, parsedSpace, parsedRooms);
                Guid? newListingId = await _apiService.CreateListingAsync(request);
                
                if (newListingId.HasValue)
                {
                    if (SelectedPhotos.Any())
                    {
                        bool allPhotosUploaded = await _apiService.UploadListingPhotosAsync(newListingId.Value, SelectedPhotos);
                        
                        if (allPhotosUploaded)
                        {
                            _dialogService.ShowInfo("Оголошення та всі фото успішно збережено!");
                        }
                        else
                        {
                            _dialogService.ShowWarning("Оголошення створено, але деякі фото не вдалося завантажити.");
                        }
                    }
                    else
                    {
                        _dialogService.ShowInfo("Оголошення успішно створено (без фото)!");
                    }
                    
                    _navigationService.NavigateWorkplaceTo<SearchView>();
                }
                else
                {
                    _dialogService.ShowError("Сервер відхилив створення оголошення");
                }
            }
            catch(HttpRequestException httpEx)
            {
                _dialogService.ShowError($"Не вдалося з'єднатися з сервером. Деталі: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Сталася непередбачувана помилка: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteSelectPhotos(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Зображення (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    if (!SelectedPhotos.Contains(fileName))
                        SelectedPhotos.Add(fileName);
                }
            }
        }
        
        private bool CanExecuteCreateListing(object parameter) => !IsLoading;
    }
}