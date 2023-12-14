using Domain.Models.Base;

namespace Queries.Informations.News.EditNews;

/// <summary>
/// Редактирование новости
/// </summary>
public interface IEditNews
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
    /// <param name="id"></param>
    /// <returns></returns>
    bool ValidateRequest(string? title, string? introduction, long? typeId, long? id);

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    string BuilderUrl(long? id);

    /// <summary>
    /// Построение тела запроса
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <param name="ordinalNumber"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    StringContent BuilderBody(string? title, string? introduction, long? typeId, long? ordinalNumber, long? id);

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
    /// <param name="id"></param>
    /// <returns></returns>
    Task<BaseResponse> Handler(string? title, string? introduction, long? typeId, long? ordinalNumber, long? id);
}