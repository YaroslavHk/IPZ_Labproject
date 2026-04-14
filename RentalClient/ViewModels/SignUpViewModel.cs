using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class SignUpViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }
        
        private string _phone;

        public string Phone
        {
            get => _phone; 
            set { _phone = value; OnPropertyChanged(); }
        }
        
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand SwitchSignCommand { get; }
        public ICommand BackToMainWindowCommand { get; }

        public SignUpViewModel(INavigationService navigationService, IRentalApiService apiService)
        {
            _navigationService = navigationService;
            _apiService = apiService;

            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            SwitchSignCommand = new RelayCommand(o => _navigationService.NavigateRootTo<SignInView>());
            BackToMainWindowCommand = new RelayCommand(o => _navigationService.NavigateRootTo<MainWindowView>());
        }

        private async void ExecuteRegister(object parameter)
        {
            var passwordBoxes = parameter as object[];
            if (passwordBoxes == null || passwordBoxes.Length < 2) return;

            var passBox1 = passwordBoxes[0] as PasswordBox;
            var passBox2 = passwordBoxes[1] as PasswordBox;

            string password = passBox1?.Password;
            string passwordConfirmation = passBox2?.Password;

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(UserName) || 
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirmation))
            {
                MessageBox.Show("Заповніть пусті поля", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != passwordConfirmation)
            {
                MessageBox.Show("Паролі відрізняються", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var registerRequestData = new RegisterRequest(Email, UserName, Phone, password);
                bool isSuccess = await _apiService.RegisterAsync(registerRequestData);

                if (isSuccess)
                {
                    var loginRequest = new AuthRequest(UserName, password);
                    bool loginSuccess = await _apiService.LoginAsync(loginRequest);
                    
                    if (loginSuccess)
                    {
                        _navigationService.NavigateRootTo<MainWindowView>();
                        _navigationService.NavigateWorkplaceTo<ProfileView>();
                    }
                    else
                    {
                        _navigationService.NavigateRootTo<MainWindowView>();
                    }
                }
                else
                {
                    MessageBox.Show("Сервер відхилив реєстрацію", "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteRegister(object parameter)
        {
            return !IsLoading;
        }
    }
}