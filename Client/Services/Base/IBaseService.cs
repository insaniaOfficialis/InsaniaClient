using Client.Models.Identification.Users.Response;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services.Base;

/// <summary>
/// Интерфейс базового сервиса
/// </summary>
public interface IBaseService
{
    /// <summary>
    /// Проверка соединения с сервером
    /// </summary>
    /// <returns></returns>
    Task CheckConnection();

    /// <summary>
    /// Обработка ошибок
    /// </summary>
    /// <param name="message"></param>
    void SetError(string message);

    /// <summary>
    /// Получение информации о пользователе
    /// </summary>
    /// <returns></returns>
    Task<UserInfoResponse> GetUserInfo();

    /// <summary>
    /// Метод получения настроек json
    /// </summary>
    /// <returns></returns>
    JsonSerializerOptions GetJsonSettings();
}
