﻿using System.Collections.Generic;

namespace Client.Models.Base;

/// <summary>
/// Модель стандартного ответа для списка
/// </summary>
public class BaseResponseList : BaseResponse
{
    /// <summary>
    /// Пустой конструктор
    /// </summary>
    public BaseResponseList()
    {
        
    }

    /// <summary>
    /// Простой конструктор ответа модели ответа списка
    /// </summary>
    /// <param name="success"></param>
    /// <param name="error"></param>
    public BaseResponseList(bool success, BaseError? error) : base(success, error)
    {

    }

    /// <summary>
    /// Конструктор с элементами модели ответа списка
    /// </summary>
    /// <param name="success"></param>
    /// <param name="error"></param>
    /// <param name="items"></param>
    public BaseResponseList(bool success, BaseError? error, List<BaseResponseListItem?>? items) : base(success, error)
    {
        Items = items;
    }

    /// <summary>
    /// Список
    /// </summary>
    public List<BaseResponseListItem?>? Items { get; set; }
}

/// <summary>
/// Модель элемента списка
/// </summary>
public class BaseResponseListItem
{
    /// <summary>
    /// Первичный ключ
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// Наименование
    /// </summary>
    public string? Name { get; set; }
}
