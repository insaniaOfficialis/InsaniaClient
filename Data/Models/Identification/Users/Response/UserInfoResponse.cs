using Domain.Models.Base;

namespace Domain.Models.Identification.Users.Response;

/// <summary>
/// Модель ответа информации о пользователе
/// </summary>
public class UserInfoResponse : BaseResponse
{
    /// <summary>
    /// Логин
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Отчество
    /// </summary>
    public string? Patronymic { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Инициалы
    /// </summary>
    public string? Initials { get; set; }

    /// <summary>
    /// Пол (истина - мужской/ложь - женский)
    /// </summary>
    public bool Gender { get; set; }

    /// <summary>
    /// Почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Признак заблокированного пользователя
    /// </summary>
    public bool? IsBlocked { get; set; }

    /// <summary>
    /// Роли
    /// </summary>
    public List<string>? Roles { get; set; }

    /// <summary>
    /// Список прав доступа
    /// </summary>
    public List<string>? AccessRights { get; set; }

    /// <summary>
    /// Список файлов пользователя
    /// </summary>
    public List<long?>? Files { get; set; }
}