using Domain.Models.Informations.NewsDetails.Response;

namespace Queries.Informations.News.GetNewsDetailsFull;

/// <summary>
/// Получение всех детальных частей новости
/// </summary>
public interface IGetNewsDetailsFull
{
    /// <summary>
    /// Проверка конфигурации
    /// </summary>
    /// <returns></returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    string BuilderUrl(long? id);

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    string CreateQueryString(long? id);

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
    bool ValidateData(GetNewsDetailsFullResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<GetNewsDetailsFullResponse> Handler(long? id);
}