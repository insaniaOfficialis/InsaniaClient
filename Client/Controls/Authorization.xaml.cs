using Client.Controls;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls
{
    /// <summary>
    /// Логика взаимодействия для Authoriztion.xaml
    /// </summary>
    public partial class Authorization : UserControl
    {
        string connectionString;

        public Authorization()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            /*Определяем действие для кнопки esc*/
            this.PreviewKeyDown += new KeyEventHandler(Enter);
        }

        /// <summary>
        /// Обработка авторизации по enter
        /// </summary>
        public void Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ButtonLogin_Click(sender, e);
        }

        private async void ButtonLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextBoxLogin.Text))
            {
                Console.WriteLine("Не указан логин");
                return;
            }

            if (String.IsNullOrEmpty(TextBoxPassword.Text))
            {
                Console.WriteLine("Не указан пароль");
                return;
            }

            /*Создаём подключение*/
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                /*Открываем подключение*/
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    string sqlExpression = "SELECT 0 FROM rUsers WHERE login = '"
                        + TextBoxLogin.Text
                        + "' AND password = '"
                        + TextBoxPassword.Text
                        + "'";

                    SqlCommand command = new SqlCommand(sqlExpression, connection);

                    /*Выполняем команду*/
                    SqlDataReader reader = command.ExecuteReader();

                    if(reader.HasRows)
                    {
                        Main main = new Main();

                        this.Content = main;
                    }
                }
            }
        }

        private void TextBoxLogin_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TextBoxLogin.Text == "Логин")
                TextBoxLogin.Text = "";
        }

        private void TextBoxPassword_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TextBoxPassword.Text == "Пароль")
                TextBoxPassword.Text = "";
        }
    }
}