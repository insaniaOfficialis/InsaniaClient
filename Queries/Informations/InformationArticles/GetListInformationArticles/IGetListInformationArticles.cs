using Domain.Models.Base;

namespace Queries.Informations.InformationArticles.GetListInformationArticles;

/// <summary>
/// Получение списка информационных статей
/// </summary>
public interface IGetListInformationArticles
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
    bool ValidateData(BaseResponseList? response);

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    Task<BaseResponseList> Handler(string? search);
}