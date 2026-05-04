using System.Linq;
using System.Windows;
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
        private readonly IDialogService _dialogService;

        private string _email = "";
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); ValidateForm(); } }

        private string _userName = "";
        public string UserName { get => _userName; set { _userName = value; OnPropertyChanged(); ValidateForm(); } }
        
        private string _phone = "";
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); ValidateForm(); } }
        
        private string _currentPassword = "";
        public string CurrentPassword { get => _currentPassword; set { _currentPassword = value; ValidateForm(); } }

        private string _currentConfirmPassword = "";
        public string CurrentConfirmPassword { get => _currentConfirmPassword; set { _currentConfirmPassword = value; ValidateForm(); } }

        private string _passwordErrorMessage;
        public string PasswordErrorMessage { get => _passwordErrorMessage; set { _passwordErrorMessage = value; OnPropertyChanged(); } }

        private bool _isFormValid;

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

        public SignUpViewModel(INavigationService navigationService, IRentalApiService apiService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _dialogService = dialogService;

            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            SwitchSignCommand = new RelayCommand(o => _navigationService.NavigateRootTo<SignInView>());
            BackToMainWindowCommand = new RelayCommand(o => _navigationService.NavigateRootTo<MainWindowView>());
        }

        private void ValidateForm()
        {
            PasswordErrorMessage = "";

            bool isEmailValid = !string.IsNullOrWhiteSpace(Email) && Email.Contains("@");
            bool isUserValid = !string.IsNullOrWhiteSpace(UserName);

            bool isPasswordLongEnough = CurrentPassword.Length >= 8;
            bool hasDigitOrSymbol = CurrentPassword.Any(char.IsDigit) || CurrentPassword.Any(char.IsPunctuation) || CurrentPassword.Any(char.IsSymbol);
            bool passwordsMatch = CurrentPassword == CurrentConfirmPassword;

            if (!string.IsNullOrEmpty(CurrentPassword))
            {
                if (!isPasswordLongEnough)
                {
                    PasswordErrorMessage = "Пароль має містити мінімум 8 символів";
                }
                else if (!hasDigitOrSymbol)
                {
                    PasswordErrorMessage = "Потрібно використовувати цифри або символи";
                }
                else if (!string.IsNullOrEmpty(CurrentConfirmPassword) && !passwordsMatch)
                {
                    PasswordErrorMessage = "Паролі не співпадають";
                }
            }

            _isFormValid = isEmailValid && isUserValid && isPasswordLongEnough && hasDigitOrSymbol && passwordsMatch;
            
            ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
        }

        private bool CanExecuteRegister(object parameter) => _isFormValid && !IsLoading;

        private async void ExecuteRegister(object parameter)
        {
            IsLoading = true;

            try
            {
                var registerRequestData = new RegisterRequest(Email, UserName, Phone, CurrentPassword);
                bool isSuccess = await _apiService.RegisterAsync(registerRequestData);

                if (isSuccess)
                {
                    var loginRequest = new AuthRequest(UserName, CurrentPassword);
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
                    _dialogService.ShowError("Сервер відхилив реєстрацію. Можливо, такий Email вже існує.");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}