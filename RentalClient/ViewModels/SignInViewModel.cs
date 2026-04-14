using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RentalClient.Core;
using RentalClient.Services;
using RentalClient.UserControls;

namespace RentalClient.ViewModels
{
    public class SignInViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IRentalApiService _apiService;

        private string _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        // Команды
        public ICommand LoginCommand { get; }
        public ICommand SwitchSignCommand { get; }
        public ICommand BackToMainWindowCommand { get; }

        public SignInViewModel(INavigationService navigationService, IRentalApiService apiService)
        {
            _navigationService = navigationService;
            _apiService = apiService;

            // Инициализация команд
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            SwitchSignCommand = new RelayCommand(o => _navigationService.NavigateRootTo<SignUpView>());
            BackToMainWindowCommand = new RelayCommand(o => _navigationService.NavigateRootTo<MainWindowView>());
        }

        private async void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заповніть пусті поля", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var authRequestData = new AuthRequest(Login, password);
                bool isSuccess = await _apiService.LoginAsync(authRequestData);

                if (isSuccess)
                {
                    _navigationService.NavigateRootTo<MainWindowView>();
                    _navigationService.NavigateWorkplaceTo<ProfileView>();
                }
                else
                {
                    MessageBox.Show("Сервер відхилив авторизацію. Перевірте данні.", "Помилка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !IsLoading;
        }
    }
}