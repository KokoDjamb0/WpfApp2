using Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Determine which window to open based on username
            if (IsValidUser(username, password))
            {
                if (username == "admin")
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                
               

                // Close the login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Неправильный логин или пароль. Пожалуйста, попробуйте снова.");
            }
        }


        private bool IsValidUser(string username, string password)
        {
            // Simple check for two users
            return (username == "admin" && password == "admin") 
                   ;
        }


    }
}
