namespace Queries.General.Files.GetFileUrl;

/// <summary>
/// Получение ссылки на файл
/// </summary>
public interface IGetFileUrl
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
    /// <param name="entityId"></param>
    /// <returns></returns>
    string BuilderUrl(long id, long entityId);
}