using Domain.Models.Base;
using Domain.Models.Informations.News.Response;

namespace Queries.Informations.News.GetNewsTypes;

/// <summary>
/// Получение списка типов новостей
/// </summary>
public interface IGetNewsTypes
{
    /// <summary>
    /// Проверка конфигурации
    /// </summary>
    /// <returns></returns>
    bool ValidateConfiguration();

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <returns></returns>
    string BuilderUrl();

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
    /// <returns></returns>
    Task<BaseResponseList> Handler();
}