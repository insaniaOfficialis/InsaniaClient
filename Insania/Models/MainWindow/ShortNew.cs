using System;

namespace Insania;

/// <summary>
/// Модель новости для списка
/// </summary>
public class ShortNew
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ShortBody { get; set; }
}
