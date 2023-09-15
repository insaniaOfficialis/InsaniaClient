using Client.Models.Base;

namespace Client.Models.Identification.Authorization.Response;

/// <summary>
/// Модель ответа авторизации
/// </summary>
public class AuthorizationResponse: BaseResponse
{
    /// <summary>
    /// Токен доступа
    /// </summary>
    public string? Token { get; set; }
}
