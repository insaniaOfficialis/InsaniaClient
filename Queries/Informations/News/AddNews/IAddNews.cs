using Domain.Models.Base;
using Domain.Models.Informations.News.Request;

namespace Queries.Informations.News.AddNews;

/// <summary>
/// Добавление новости
/// </summary>
public interface IAddNews
{
    /// <summary>
    /// Проверка конфигурации
    /// </summary>
    /// <returns></returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Проверка запроса
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <returns></returns>
    bool ValidateRequest(string? title, string? introduction, long? typeId);

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <returns></returns>
    string BuilderUrl();

    /// <summary>
    /// Построение тела запроса
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <param name="ordinalNumber"></param>
    /// <returns></returns>
    StringContent BuilderBody(string? title, string? introduction, long? typeId, long? ordinalNumber);

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
    bool ValidateData(BaseResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <param name="ordinalNumber"></param>
    /// <returns></returns>
    Task<BaseResponse> Handler(string? title, string? introduction, long? typeId, long? ordinalNumber);
}