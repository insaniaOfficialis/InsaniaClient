namespace Domain.Models.Base;

/// <summary>
/// Базовая модель сортировки
/// </summary>
public class BaseSortRequest
{
    /// <summary>
    /// Конструктор базовой модели сортировки
    /// </summary>
    /// <param name="sortKey"></param>
    /// <param name="isAscending"></param>
    public BaseSortRequest(string sortKey, bool isAscending)
    {
        SortKey = sortKey;
        IsAscending = isAscending;
    }

    /// <summary>
    /// Поле для сортировки
    /// </summary>
    public string? SortKey { get; set; }

    /// <summary>
    /// Порядок сортировки
    /// </summary>
    public bool? IsAscending { get; set; }
}
