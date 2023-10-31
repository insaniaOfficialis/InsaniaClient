using Domain.Models.Informations.News.Response;

namespace Queries.Informations.News.GetListNews;

/// <summary>
/// Получение списка новостей
/// </summary>
public interface IGetListNews
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
    /// <returns></returns>
    string BuilderUrl(string? search);

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    string CreateQueryString(string? search);

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
    bool ValidateData(GetNewsListResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    Task<GetNewsListResponse> Handler(string? search);
}