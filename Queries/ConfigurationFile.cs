using System.Reflection;
using System.Xml.Serialization;

namespace Queries;

/// <summary>
/// Класс конфигурации
/// </summary>
public class ConfigurationFile
{
    public Configuration? _configuration; //рбхект конфигурации

    /// <summary>
    /// Конструктор класс конфигурации
    /// </summary>
    public ConfigurationFile()
    {
        //Формируем путь к файлу конфигурации
        string? location = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = location + "\\Client.dll.config";

        //Формируем сериализатор
        XmlSerializer xmlSerializer = new(typeof(Configuration));

        //Считываем файл
        using FileStream fs = new(filePath, FileMode.OpenOrCreate);

        //Записываем десериализованный файл
        _configuration = xmlSerializer.Deserialize(fs) as Configuration;
    }

    /// <summary>
    /// Метод получения значения по ключу
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetValue(string? key)
    {
        return _configuration?.AppSettings?.Elements?.FirstOrDefault(x => x.Key == key)?.Value;
    }

    public void SetValue(string? key, string value)
    {
        if(_configuration != null && _configuration.AppSettings != null && _configuration.AppSettings.Elements != null)
            _configuration.AppSettings.Elements.First(x => x.Key == key).Value = value;
        
        //Формируем путь к файлу конфигурации
        string? location = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = location + "\\Client.dll.config";

        //Формируем сериализатор
        XmlSerializer xmlSerializer = new(typeof(Configuration));
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        //Считываем файл
        using FileStream fs = new(filePath, FileMode.OpenOrCreate);

        //Записываем сериализованный файл
        xmlSerializer.Serialize(fs, _configuration, ns);
    }
}

/// <summary>
/// Модель файла конфигурации
/// </summary>
[Serializable]
[XmlRoot(ElementName = "configuration")]
public class Configuration
{
    [XmlElement(ElementName = "appSettings")]
    public AppSettings? AppSettings { get; set; }

    /*[XmlElement(ElementName = "startup")]
    public Startup? Startup { get; set; }

    [XmlElement(ElementName = "connectionStrings")]
    public ConnectionStrings? ConnectionStrings { get; set; }*/
}

/// <summary>
/// Модель startup
/// </summary>
/*[Serializable]
[XmlRoot(ElementName = "startup")]
public class Startup
{
    [XmlElement(ElementName = "supportedRuntime")]
    public SupportedRuntime? SupportedRuntime { get; set; }
}

/// <summary>
/// Модель supportedRuntime
/// </summary>
[Serializable]
[XmlRoot(ElementName = "supportedRuntime")]
public class SupportedRuntime
{
    [XmlElement(ElementName = "version")]
    public string? Version { get; set; }

    [XmlElement(ElementName = "sku")]
    public string? Sku { get; set; }
}

/// <summary>
/// Модель строк подключения
/// </summary>
[Serializable]
[XmlRoot(ElementName = "connectionStrings")]
public class ConnectionStrings
{
    [XmlElement(ElementName = "add")]
    public ConnectionStringValue? ConnectionString { get; set; }
}

/// <summary>
/// Модель элемента строки подключения
/// </summary>
[Serializable]
[XmlRoot(ElementName = "add")]
public class ConnectionStringValue
{
    [XmlElement(ElementName = "name")]
    public string? Name { get; set; }

    [XmlElement(ElementName = "connectionString")]
    public string? ConnectionString { get; set; }

    [XmlElement(ElementName = "providerName")]
    public string? ProviderName { get; set; }
}*/

/// <summary>
/// Модель appsetings
/// </summary>
[Serializable]
[XmlRoot(ElementName = "appSettings")]
public class AppSettings
{
    [XmlElement(ElementName = "add")]
    public List<Element>? Elements { get; set; }
}

/// <summary>
/// Модель элемента
/// </summary>
[Serializable]
[XmlRoot(ElementName = "add")]
public class Element
{
    /// <summary>
    /// Ключ
    /// </summary>
    [XmlAttribute(AttributeName = "key")]
    public string? Key { get; set; }

    /// <summary>
    /// Значение
    /// </summary>
    [XmlAttribute(AttributeName = "value")]
    public string? Value { get; set; }
}