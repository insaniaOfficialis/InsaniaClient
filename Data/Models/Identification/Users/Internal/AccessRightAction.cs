namespace Domain.Models.Identification.Users.Internal;

/// <summary>
/// Модель прав доступа действий
/// </summary>
public class AccessRightAction
{
    /// <summary>
    /// Пустой конструктор модели прав доступа действий
    /// </summary>
    public AccessRightAction()
    {
        
    }

    /// <summary>
    /// Конструктор модели прав доступа действий
    /// </summary>
    /// <param name="edit"></param>
    /// <param name="delete"></param>
    /// <param name="restore"></param>
    public AccessRightAction(bool edit, bool delete, bool restore)
    {
        Edit = edit;
        Delete = delete;
        Restore = restore;
    }

    /// <summary>
    /// Редактирование
    /// </summary>
    public bool? Edit { get; set; }

    /// <summary>
    /// Удаление
    /// </summary>
    public bool? Delete { get; set; }

    /// <summary>
    /// Восстановление
    /// </summary>
    public bool? Restore { get; set; }
}
