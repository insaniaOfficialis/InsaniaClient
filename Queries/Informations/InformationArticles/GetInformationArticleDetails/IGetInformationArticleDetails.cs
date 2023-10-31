using Domain.Models.Informations.InformationArticlesDetails.Response;

namespace Queries.Informations.InformationArticles.GetInformationArticleDetails;

/// <summary>
/// Получение списка детальных частей информационной статьи
/// </summary>
public interface IGetInformationArticleDetails
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
    bool ValidateData(GetInformationArticleDetailsResponse? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<GetInformationArticleDetailsResponse> Handler(long? id);
}