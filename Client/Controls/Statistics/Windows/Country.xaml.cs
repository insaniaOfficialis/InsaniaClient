using Client.Models.Base;
using Domain.Models.Politics.Countries.Request;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Statistics.Windows
{
    /// <summary>
    /// Логика взаимодействия для Country.xaml
    /// </summary>
    public partial class Country : Window
    {
        long _id; //id записи
        public ILogger _logger { get { return Log.ForContext<Country>(); } } //логгер для записи логов
        readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

        /// <summary>
        /// Конструктор окна
        /// </summary>
        public Country()
        {
            try
            {
                //Инициализируем компоненты
                InitializeComponent();

                //Выставляем параметры десериализации
                _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                //Проверяем доступность api
                CheckConnection();
            }
            catch (Exception ex)
            {
                _logger.Error("Country. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Конструктор окна для редактирования
        /// </summary>
        /// <param name="id"></param>
        /// <param name="number"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="language"></param>
        public Country(long id, int number, string name, string color, string language)
        {
            try
            {
                //Инициализируем компоненты
                InitializeComponent();

                //Заполняем поля ввода
                RowNumberTextBox.Text = number.ToString();
                NameTextBox.Text = name;
                ColorTextBox.Text = color;
                LanguageForNamesTextBox.Text = language;

                //Заполняем id
                _id = id;

                //Выставляем параметры десериализации
                _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                //Проверяем доступность api
                CheckConnection();
            }
            catch (Exception ex)
            {
                _logger.Error("Country. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод обнуления полей ввода по нажатию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textbox = sender as TextBox;

                switch (textbox.Name)
                {
                    case "RowNumberTextBox":
                        {
                            if (RowNumberTextBox.Text == "Номер на карте")
                                RowNumberTextBox.Text = "";
                        }
                        break;
                    case "NameTextBox":
                        {
                            if (NameTextBox.Text == "Наименование")
                                NameTextBox.Text = "";
                        }
                        break;
                    case "ColorTextBox":
                        {
                            if (ColorTextBox.Text == "Цвет на карте")
                                ColorTextBox.Text = "";
                        }
                        break;
                    case "LanguageForNamesTextBox":
                        {
                            if (LanguageForNamesTextBox.Text == "Язык для названий")
                                LanguageForNamesTextBox.Text = "";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Country. TextBox_GotFocus. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод возвращения значений по умолчанию полей ввода по потере фокуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var textbox = sender as TextBox;

                switch (textbox.Name)
                {
                    case "RowNumberTextBox":
                        {
                            if (RowNumberTextBox.Text == "")
                                RowNumberTextBox.Text = "Номер на карте";
                        }
                        break;
                    case "NameTextBox":
                        {
                            if (NameTextBox.Text == "")
                                NameTextBox.Text = "Наименование";
                        }
                        break;
                    case "ColorTextBox":
                        {
                            if (ColorTextBox.Text == "")
                                ColorTextBox.Text = "Цвет на карте";
                        }
                        break;
                    case "LanguageForNamesTextBox":
                        {
                            if (LanguageForNamesTextBox.Text == "")
                                LanguageForNamesTextBox.Text = "Язык для названий";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Country. TextBox_LostFocus. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод проверки соединения с api
        /// </summary>
        public async void CheckConnection()
        {
            try
            {
                //Блокируем все элементы
                RowNumberTextBox.IsEnabled = false;
                NameTextBox.IsEnabled = false;
                ColorTextBox.IsEnabled = false;
                LanguageForNamesTextBox.IsEnabled = false;
                SaveButton.IsEnabled = false;

                //Объявляем переменную ссылки запроса
                string url = null;

                try
                {
                    //Если в конфиге есть данные для формирования ссылки запроса
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"]))
                    {
                        //Формируем ссылку запроса
                        url = ConfigurationManager.AppSettings["DefaultConnection"];

                        //Получаем данные по запросу
                        using HttpClient client = new();

                        using var result = await client.GetAsync(url);

                        //Если получили успешный результат
                        if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            //Разблокируем все элементы
                            RowNumberTextBox.IsEnabled = true;
                            NameTextBox.IsEnabled = true;
                            ColorTextBox.IsEnabled = true;
                            LanguageForNamesTextBox.IsEnabled = true;
                            SaveButton.IsEnabled = true;
                        }
                        //Иначе возвращаем ошибку
                        else
                            SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
                    }
                    //Иначе возвращаем ошибку
                    else
                        SetError("Не указан адрес api. Обратитесь в техническую поддержку", true);
                }
                catch
                {
                    SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
                }
            }
            catch (Exception ex)
            {
                SetError(ex.Message, true);
            }
        }

        /// <summary>
        /// Метод отображения ошибок
        /// </summary>
        /// <param name="message"></param>
        /// <param name="criticalException"></param>
        public void SetError(string message, bool criticalException)
        {
            try
            {
                //Объявляем переменные
                string style; //стиль

                //Определяем наименование стиля
                if (criticalException)
                    style = "CriticalExceptionTextBlock";
                else
                    style = "InnerExceptionTextBlock";

                //Находим стиль
                var serverExceptionStyle = FindResource(style) as Style;

                //Устанавливаем текст и стиль
                ErrorText.Style = serverExceptionStyle;
                ErrorText.Text = message;

                //Включаем кнопку сохранения
                SaveButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error("Country. SetError. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод изменения цвета
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(ColorTextBox.Text) && ColorTextBox.Text != "Цвет на карте")
                {
                    string color = String.Empty;

                    //Если первый символ не решётка, добавляем решётку
                    if (ColorTextBox.Text[0] != '#')
                        color = "#" + ColorTextBox.Text.Substring(0);

                    if (!String.IsNullOrEmpty(color))
                        ColorTextBox.Text = color;

                    ColorTextBox.CaretIndex = ColorTextBox.Text.Length;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Country. ColorTextBox_TextChanged. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод сохранения
        /// </summary>
        private async void Save()
        {
            try
            {
                //Отключаем кнопку для нажатия
                SaveButton.IsEnabled = false;

                //Убираем тест ошибки
                ErrorText.Text = null;

                //Проверяем ошибки
                if (String.IsNullOrEmpty(RowNumberTextBox.Text) || RowNumberTextBox.Text == "Номер на карте")
                {
                    SetError("Не указан номер на карте", false);
                    return;
                }
                if (String.IsNullOrEmpty(NameTextBox.Text) || NameTextBox.Text == "Наименование")
                {
                    SetError("Не указано наименование", false);
                    return;
                }
                if (String.IsNullOrEmpty(ColorTextBox.Text) || ColorTextBox.Text == "Цвет на карте")
                {
                    SetError("Не указана цвет на карте", false);
                    return;
                }
                if (ColorTextBox.Text.Length != 7 || !ColorTextBox.Text.StartsWith("#"))
                {
                    SetError("Некорректно указан цвет на карте", false);
                    return;
                }
                if (String.IsNullOrEmpty(LanguageForNamesTextBox.Text) || LanguageForNamesTextBox.Text == "Язык для названий")
                {
                    SetError("Не указан язык для названий", false);
                    return;
                }

                AddCountryRequest request = new(NameTextBox.Text, Convert.ToInt32(RowNumberTextBox.Text), ColorTextBox.Text, LanguageForNamesTextBox.Text);

                //Если в конфиге есть данные для формирования ссылки запроса
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
                {
                    //Формируем ссылку запроса
                    string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                        ConfigurationManager.AppSettings["Countries"];

                    if (_id <= 0)
                        url += "add";
                    else
                        url += "update/" + _id;

                    //Формируем клиента, добавляем ему токен и тело запроса
                    using HttpClient client = new();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);
                    StringContent stringCintent = new(JsonSerializer.Serialize(request, _settings).ToString(), Encoding.UTF8, "application/json");

                    //Получаем результат запроса
                    HttpResponseMessage result;
                    if (_id <= 0)
                        result = await client.PostAsync(url, stringCintent);
                    else
                        result = await client.PutAsync(url, stringCintent);

                    //Если получили успешный результат
                    if (result != null)
                    {
                        if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            SetError("Некорректный токен", false);

                        //Десериализуем ответ
                        var content = await result.Content.ReadAsStringAsync();

                        BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

                        //Если успешно, зыкрываем окно
                        if (response.Success && response.Id != null)
                        {
                            Close();
                            Message message = new("Успешно");
                            message.Show();
                        }
                        else
                            SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                    }
                    else
                        SetError("Ошибка сервера", true);
                }
                //Иначе возвращаем ошибку
                else
                    SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);

                //Включаем кнопку для нажатия
                SaveButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                SetError("Не удалось сохранить. Обратитесь в техническую поддержку", true);
                _logger.Error("Country. Save. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Событие нажатия клавиши сохранить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Вызываем сохранение
                Save();
            }
            catch (Exception ex)
            {
                _logger.Error("Country. SaveButton_Click. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Событие нажатия клавиш
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                //Если нажата клавиша eacape
                if (e.Key == Key.Escape)
                    //Закрываем окно
                    Close();

                //Если нажата клавиша enter
                if (e.Key == Key.Enter)
                    //Вызываем сохранение
                    Save();
            }
            catch (Exception ex)
            {
                _logger.Error("Country. Window_PreviewKeyDown. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Событие перетягивания мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //Если левая кнопка мыши
                if (e.ChangedButton == MouseButton.Left)
                    //Включаем перетягивание
                    DragMove();
            }
            catch (Exception ex)
            {
                _logger.Error("Country. Window_MouseDown. Ошибка: {0}", ex);
            }
        }
    }
}
