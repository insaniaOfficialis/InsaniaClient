using Domain.Models.Base;
using Domain.Models.Informations.News.Response;

namespace Queries.Informations.News.GetNewsTable;

/// <summary>
/// Получение списка новостей для таблицы
/// </summary>
public interface IGetNewsTable
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
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    string BuilderUrl(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted);

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    string CreateQueryString(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted);

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
    bool ValidateData(GetNewsTableResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    Task<GetNewsTableResponse> Handler(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted);
}