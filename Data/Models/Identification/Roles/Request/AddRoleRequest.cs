namespace Domain.Models.Identification.Roles.Request;

/// <summary>
/// Модель запроса добавления ролей
/// </summary>
public class AddRoleRequest
{
    /// <summary>
    /// Конструктор модели запроса добавления ролей
    /// </summary>
    /// <param name="name"></param>
    public AddRoleRequest(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Наименование роли
    /// </summary>
    public string? Name { get; set; }
}
