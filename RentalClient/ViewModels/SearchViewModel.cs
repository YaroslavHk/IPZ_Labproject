using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class SearchViewModel : ObservableObject, IDisposable
    {
        private readonly IRentalApiService _apiService;
        private readonly ISearchStateService _searchState;
        private readonly DispatcherTimer _pollingTimer;
        private readonly IListingStateService _listingState;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        
        private ObservableCollection<ShortRentalResponse> _rentals = new();
        public ObservableCollection<ShortRentalResponse> Rentals
        {
            get => _rentals;
            set 
            { 
                _rentals = value; 
                OnPropertyChanged(); 
            }
        }

        
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
        
        private bool _priceSort;
        public bool PriceSort 
        { 
            get => _priceSort; 
            set 
            { 
                _priceSort = value; 
                OnPropertyChanged(); 
                if (value) _ = FetchDataAsync(showMessages: false); 
            } 
        }
        
        private bool _spaceSort;
        public bool SpaceSort 
        { 
            get => _spaceSort; 
            set 
            { 
                _spaceSort = value; 
                OnPropertyChanged(); 
                if (value) _ = FetchDataAsync(showMessages: false); 
            } 
        }
        
        private bool _roomsCountSort;
        public bool RoomsCountSort 
        { 
            get => _roomsCountSort; 
            set 
            { 
                _roomsCountSort = value; 
                OnPropertyChanged(); 
                if (value) _ = FetchDataAsync(showMessages: false); 
            } 
        }
        
        private bool _isSortDescending;
        public bool IsSortDescending
        {
            get => _isSortDescending;
            set
            {
                if (_isSortDescending != value)
                {
                    _isSortDescending = value;
                    OnPropertyChanged();
            
                    if (PriceSort || SpaceSort || RoomsCountSort)
                    {
                        _ = FetchDataAsync(showMessages: false);
                    }
                }
            }
        }
        
        private bool _isLoading;
        public bool IsLoading 
        { 
            get => _isLoading; 
            set { _isLoading = value; OnPropertyChanged(); } 
        }

        public ICommand ClearFiltersCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand OpenListingCommand { get; }

        public SearchViewModel(IRentalApiService apiService, ISearchStateService searchState, IListingStateService listingState, INavigationService navigationService, IDialogService  dialogService)
        {
            _apiService = apiService;
            _searchState = searchState;
            _listingState = listingState;
            _navigationService = navigationService;
            _dialogService = dialogService;
            
            
            ApplyFilterCommand = new RelayCommand(ExecuteApplyFilter);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            OpenListingCommand = new RelayCommand(ExecuteOpenListing);
            
            _searchState.SearchRequested += OnGlobalSearchRequested;
            
            _pollingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(120) 
            };
            
            _pollingTimer.Tick += OnTimerTick;
            _pollingTimer.Start();
        
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.InvokeAsync(async () => 
                {
                    await LoadInitialDataAsync();
                }, DispatcherPriority.Loaded);
            }
            
        }
        
        private void OnGlobalSearchRequested()
        {
            ExecuteApplyFilter(null);
        }
        
        private async void OnTimerTick(object sender, EventArgs e)
        {
            await FetchDataAsync(showMessages: false);
        }
        
        private async Task LoadInitialDataAsync()
        {
            await FetchDataAsync(showMessages: false);
        }

        private async void ExecuteApplyFilter(object parameter)
        {
            await FetchDataAsync(showMessages: true);
        }

        private async Task FetchDataAsync(bool showMessages)
        {
            if (!TryParseFilters(out var filters))
            {
                if (showMessages) _dialogService.ShowWarning("Будь ласка, перевірте правильність введення числових значень.");
                return;
            }
            
            IsLoading = true;
            Rentals.Clear();
            
            string? title = string.IsNullOrWhiteSpace(_searchState.SearchText) ? null : _searchState.SearchText;
            string? type = IsApartment && !IsHouse ? "Apartment" : (!IsApartment && IsHouse ? "House" : null); 
            
            string? sortBy = null;
            if (PriceSort) sortBy = "price";
            else if (SpaceSort) sortBy = "space";
            else if (RoomsCountSort) sortBy = "rooms";

            try
            {
                var data = await _apiService.GetShortRentalByFilterAsync(
                    1, 10,
                    title,
                    filters.MinPrice, filters.MaxPrice,
                    filters.Rooms,
                    filters.MinSpace, filters.MaxSpace,
                    type,
                    sortBy,
                    IsSortDescending);

                Rentals.Clear();
                if (data != null)
                {
                    Rentals = new ObservableCollection<ShortRentalResponse>(data);
                }

                if (showMessages) _dialogService.ShowInfo($"Фільтр застосовано! Знайдено: {Rentals.Count}");
            }
            catch (Exception ex)
            {
                if (showMessages) _dialogService.ShowError($"Помилка при завантаженні: {ex.Message}");;
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public void Dispose()
        {
            if (_pollingTimer != null)
            {
                _pollingTimer.Stop();
                _pollingTimer.Tick -= OnTimerTick;
            }
        
            _searchState.SearchRequested -= OnGlobalSearchRequested;
        }
        
        private void ExecuteOpenListing(object parameter)
        {
            if (parameter is Guid listingId)
            {
                _listingState.SelectedListingId = listingId;
            
                _navigationService.NavigateWorkplaceTo<ListingView>();
            }
        }
        
        private async void ExecuteClearFilters(object parameter)
        {
            MinPrice = string.Empty;
            MaxPrice = string.Empty;
            Rooms = string.Empty;
            MinSpace = string.Empty;
            MaxSpace = string.Empty;

            IsApartment = false;
            IsHouse = false;

            _priceSort = false; OnPropertyChanged(nameof(PriceSort));
            _spaceSort = false; OnPropertyChanged(nameof(SpaceSort));
            _roomsCountSort = false; OnPropertyChanged(nameof(RoomsCountSort));
            
            _isSortDescending = false; OnPropertyChanged(nameof(IsSortDescending));
            
            await FetchDataAsync(showMessages: false); 
        }
        
        private bool TryParseFilters(out (decimal? MinPrice, decimal? MaxPrice, int? Rooms, float? MinSpace, float? MaxSpace) result)
        {
            result = default;

            var minP = ParseDecimal(_minPrice);
            var maxP = ParseDecimal(_maxPrice);
            var rms = ParseInt(_rooms);
            var minS = ParseFloat(_minSpace);
            var maxS = ParseFloat(_maxSpace);

            if (minP < 0 || (minP.HasValue && maxP.HasValue && maxP < minP)) return false;
            if (rms < 1 && rms != null) return false;
            if (minS < 0 || (minS.HasValue && maxS.HasValue && maxS < minS)) return false;

            result = (minP, maxP, rms, minS, maxS);
            return true;
        }

        private decimal? ParseDecimal(string s) => decimal.TryParse(s, out var v) ? v : null;
        private int? ParseInt(string s) => int.TryParse(s, out var v) ? v : null;
        private float? ParseFloat(string s) => float.TryParse(s, out var v) ? v : null;
    }
}