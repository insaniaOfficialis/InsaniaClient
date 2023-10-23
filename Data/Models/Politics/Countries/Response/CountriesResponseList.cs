using Domain.Models.Base;
using System.Collections.ObjectModel;

namespace Domain.Models.Politics.Countries.Response;

/// <summary>
/// Модель стандартного ответа для списка
/// </summary>
public class CountriesResponseList: BaseResponseList
{
    /// <summary>
    /// Пустой конструктор
    /// </summary>
    public CountriesResponseList()
    {
        
    }

    /// <summary>
    /// Простой конструктор ответа модели ответа списка
    /// </summary>
    /// <param name="success"></param>
    /// <param name="error"></param>
    public CountriesResponseList(bool success, BaseError? error): base(success, error)
    {
        
    }

    public CountriesResponseList(bool success, BaseError? error, ObservableCollection<CountriesResponseListItem?>? items): base(success, error)
    {
        Items = items;
    }

    /// <summary>
    /// Список
    /// </summary>
    public new ObservableCollection<CountriesResponseListItem?>? Items { get; set; }
}

/// <summary>
/// Модель элемента списка
/// </summary>
public class CountriesResponseListItem: BaseResponseListItem
{
    /// <summary>
    /// Номер на карте
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Цвет на карте
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Язык для названий
    /// </summary>
    public string? LanguageForNames { get; set; }
}