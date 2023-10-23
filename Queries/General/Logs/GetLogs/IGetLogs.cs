using Domain.Models.Base;
using Domain.Models.General.Logs.Response;

namespace Queries.General.Logs.GetLogs;

/// <summary>
/// Получение логов
/// </summary>
public interface IGetLogs
{
    /// <summary>
    /// Проверка конфигурации
    /// </summary>
    /// <returns></returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    string BuilderUrl(string? search, int? skip, int? take, List<BaseSortRequest>? sort, DateTime? from, DateTime? to,
        bool? success);

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    string CreateQueryString(string? search, int? skip, int? take, List<BaseSortRequest>? sort, DateTime? from, DateTime? to,
        bool? success);

    /// <summary>
    /// Проверка ответа
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    bool ValidateResponse(HttpResponseMessage? response);

    /// <summary>
    /// Проверка данных ответа
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    bool ValidateData(GetLogsResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    Task<GetLogsResponse> Handler(string? search, int? skip, int? take, List<BaseSortRequest>? sort, DateTime? from,
        DateTime? to, bool? success);
}