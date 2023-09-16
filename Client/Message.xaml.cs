using Serilog;
using System;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для Message.xaml
    /// </summary>
    public partial class Message : Window
    {
        public ILogger _logger { get { return Log.ForContext<Message>(); } } //логгер для записи логов

        /// <summary>
        /// Конструктор окна сообщений
        /// </summary>
        public Message()
        {
            try
            {
                /*Инициализируем компоненты*/
                InitializeComponent();
            }
            catch(Exception ex)
            {
                _logger.Error("Message. " + ex.Message);
            }
        }

        /// <summary>
        /// Конструктор окна сообщений с сообщением
        /// </summary>
        public Message(string message)
        {
            try
            {
                /*Инициализируем компоненты*/
                InitializeComponent();

                /*Заполняем текст*/
                MessageText.Text = message;
            }
            catch(Exception ex)
            {
                _logger.Error("Message. " + ex.Message);
            }
        }

        /// <summary>
        /// Событие нажатия клавиши ок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*Закрываем окно*/
                Close();
            }
            catch(Exception ex)
            {
                _logger.Error("Message. OkButton_Click. " + ex.Message);
            }
        }

        /// <summary>
        /// Событие нажатия клавиши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                /*Если нажата клавиша eacape*/
                if (e.Key == Key.Escape)
                    /*Закрываем окно*/
                    Close();
            }
            catch(Exception ex)
            {
                _logger.Error("Message. Grid_PreviewKeyDown. " + ex.Message);
            }
        }
    }
}
