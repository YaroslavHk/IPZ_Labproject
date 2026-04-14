using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;

namespace RentalClient.ViewModels
{
    public class SearchViewModel : ObservableObject
    {
        private readonly IRentalApiService _apiService;

        public ObservableCollection<ShortRentalResponse> Rentals { get; } = new ObservableCollection<ShortRentalResponse>();

        
        private string _minPrice;
        public string MinPrice { get => _minPrice; set { _minPrice = value; OnPropertyChanged(); } }

        private string _maxPrice;
        public string MaxPrice { get => _maxPrice; set { _maxPrice = value; OnPropertyChanged(); } }

        private string _rooms;
        public string Rooms { get => _rooms; set { _rooms = value; OnPropertyChanged(); } }

        private bool _isApartment;
        public bool IsApartment { get => _isApartment; set { _isApartment = value; OnPropertyChanged(); } }

        private bool _isHouse;
        public bool IsHouse { get => _isHouse; set { _isHouse = value; OnPropertyChanged(); } }

        private string _minSpace;
        public string MinSpace { get => _minSpace; set { _minSpace = value; OnPropertyChanged(); } }

        private string _maxSpace;
        public string MaxSpace { get => _maxSpace; set { _maxSpace = value; OnPropertyChanged(); } }

        
        
        public ICommand ApplyFilterCommand { get; }

        public SearchViewModel(IRentalApiService apiService)
        {
            _apiService = apiService;
            
            ApplyFilterCommand = new RelayCommand(ExecuteApplyFilter);
            
            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var data = await _apiService.GetRentals(1,50); 
                
                Rentals.Clear();
                foreach (var item in data)
                {
                    Rentals.Add(item);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message, "Помилка мережі", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Системна помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteApplyFilter(object parameter)
        {
            
            MessageBox.Show($"Фільтр застосовано! Ціна: {MinPrice}-{MaxPrice}, Кімнати: {Rooms}", "Інфо");
        }
    }
}