using Domain.Models.Base;
using System.Collections.ObjectModel;

namespace Domain.Models.General.Logs.Response;

/// <summary>
/// Модель ответа для списка логов
/// </summary>
public class GetLogsResponse : BaseResponseList
{
    /// <summary>
    /// Пустой конструктор модели ответа для списка логов с ошибкой
    /// </summary>
    public GetLogsResponse() : base()
    {
        
    }

    /// <summary>
    /// Конструктор модели ответа для списка логов с ошибкой
    /// </summary>
    /// <param name="success"></param>
    /// <param name="error"></param>
    public GetLogsResponse(bool success, BaseError? error) : base(success, error)
    {

    }

    /// <summary>
    /// Конструктор модели ответа для списка логов
    /// </summary>
    /// <param name="success"></param>
    /// <param name="error"></param>
    /// <param name="items"></param>
    public GetLogsResponse(bool success, BaseError? error, ObservableCollection<GetLogsResponseItem?>? items) : base(success, error)
    {
        Items = items;
    }

    /// <summary>
    /// Список
    /// </summary>
    public new ObservableCollection<GetLogsResponseItem?>? Items { get; set; }
}

/// <summary>
/// Модель элемента списка
/// </summary>
public class GetLogsResponseItem : BaseResponseListItem
{
    /// <summary>
    /// Первичный ключ лога
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Наименование вызываемого метода
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Тип вызываемого метода
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Признак успешного выполнения
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    public DateTime DateStart { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public DateTime? DateEnd { get; set; }

    /// <summary>
    /// Данные на вход
    /// </summary>
    public string? DataIn { get; set; }

    /// <summary>
    /// Данные на выход
    /// </summary>
    public string? DataOut { get; set; }
}