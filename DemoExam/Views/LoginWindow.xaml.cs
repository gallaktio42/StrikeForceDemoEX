using System.Windows;
using DemoExam.Services;

namespace DemoExam.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static Models.User? CurrentUser { get; private set; }
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = AuthService.Authenticate(txtLogin.Text, txtPassword.Password);
            if (user != null)
            {
                CurrentUser = user;
                OpenMainWindow();

            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = AuthService.GetGuestUser();
            if (CurrentUser == null || CurrentUser != null)
                OpenMainWindow();
            else
                MessageBox.Show("Ошибка загрузки гостевого доступа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OpenMainWindow()
        {
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
