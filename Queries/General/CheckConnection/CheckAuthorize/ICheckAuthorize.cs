namespace Queries.General.CheckConnection.CheckAuthorize;

/// <summary>
/// Проверка соединения под авторизованным пользователем
/// </summary>
public interface ICheckAuthorize
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
    /// Обработчик
    /// </summary>
    /// <returns></returns>
    Task<bool> Handler();
}