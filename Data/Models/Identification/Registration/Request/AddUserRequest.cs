using System.Collections.Generic;

namespace Domain.Models.Identification.Registration.Request;

/// <summary>
/// Модель запроса добавления пользователя
/// </summary>
public class AddUserRequest
{
    /// <summary>
    /// Конструктор модели запроса добавления пользователей
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="email"></param>
    /// <param name="phoneNumber"></param>
    /// <param name="lastName"></param>
    /// <param name="firstName"></param>
    /// <param name="patronymic"></param>
    /// <param name="gender"></param>
    /// <param name="roles"></param>
    public AddUserRequest(string username, string password, string email, string phoneNumber, string lastName,
        string firstName, string patronymic, bool gender, List<string> roles)
    {
        UserName = username;
        Password = password;
        Email = email;
        PhoneNumber = phoneNumber;
        LastName = lastName;
        FirstName = firstName;
        Patronymic = patronymic;
        Gender = gender;
        Roles = roles;
    }

    /// <summary>
    /// Логин
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Отчество
    /// </summary>
    public string Patronymic { get; set; }

    /// <summary>
    /// Пол (истина - мужской/ложь - женский)
    /// </summary>
    public bool Gender { get; private set; }

    /// <summary>
    /// Массив ролей
    /// </summary>
    public List<string> Roles { get; set; }
}
