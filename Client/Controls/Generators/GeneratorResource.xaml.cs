using Serilog;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorResource.xaml
/// </summary>
public partial class GeneratorResource : UserControl
{
    public ILogger _logger { get { return Log.ForContext<GeneratorResource>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор страницы генератора ресурсов
    /// </summary>
    public GeneratorResource()
    {
        try
        {
            /*Инициализирум список*/
            InitializeComponent();

            /*Получаем страны*/
            GetCountries();
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorResource. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод получения стран
    /// </summary>
    public void GetCountries()
    {
        try
        {
            /*Объявляем список стран*/
            List<string> CountriesList = new();

            /*Заполняем список стран*/
            CountriesList.Add("Альвраатская империя");
            CountriesList.Add("Фесгарское княжество");
            CountriesList.Add("Альтерское княжество");
            CountriesList.Add("Орлиадарская конфеерация");

            /*Присваиваем источник данных для выпадающего списка стран*/
            Countries.ItemsSource = CountriesList;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorResource. GetCountries. " + ex.Message);
        }
    }
}
