using Domain.Models.Base;
using Domain.Models.Informations.News.Request;
using Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Queries.Informations.News.AddNews;

/// <summary>
/// Добавление новости
/// </summary>
public class AddNews : IAddNews
{
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private ConfigurationFile _configuration; //класс конфигурации

    /// <summary>
    /// Добавление новости
    /// </summary>
    public AddNews()
    {
        _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        _configuration = new();
    }

    /// <summary>
    /// Проверка конфигурации
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool ValidateConfiguration()
    {
        //Проверяем данные из файла конфигурации
        if (string.IsNullOrEmpty(_configuration.GetValue("DefaultConnection")))
            throw new Exception("Не указан адрес api");

        if (string.IsNullOrEmpty(_configuration.GetValue("Api")))
            throw new Exception("Не указан адрес версии api");

        if (string.IsNullOrEmpty(_configuration.GetValue("Token")))
            throw new Exception("Не указан токен");

        if (string.IsNullOrEmpty(_configuration.GetValue("News")))
            throw new Exception("Не указан адрес сервиса новостей");

        //Возвращаем результат
        return true;
    }

    /// <summary>
    /// Проверка запроса
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool ValidateRequest(string? title, string? introduction, long? typeId)
    {
        //Проверяем заголовок
        if (string.IsNullOrEmpty(title))
            throw new Exception(Errors.EmptyTitle);
        
        //Проверяем вступление
        if (string.IsNullOrEmpty(introduction))
            throw new Exception(Errors.EmptyIntroduction);

        //Проверяем тип
        if (typeId == null)
            throw new Exception(Errors.EmptyTypeNews);

        //Возвращаем результат
        return true;
    }

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string BuilderUrl()
    {
        //Проверяем конфигурацию файла
        if (ValidateConfiguration())
        {
            //Формируем ссылку запроса
            string url = _configuration.GetValue("DefaultConnection") + _configuration.GetValue("Api")
                + _configuration.GetValue("News");

            //Возвращаем результат
            return url;
        }
        else
            throw new Exception("Не удалось пройти проверку");
    }

    /// <summary>
    /// Построение тела запроса
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <param name="ordinalNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public StringContent BuilderBody(string? title, string? introduction, long? typeId, long? ordinalNumber)
    {
        //Проверяем данные запроса
        if (ValidateRequest(title, introduction, typeId))
        {
            AddNewsRequest body = new(title!, introduction!, typeId! ?? 0, ordinalNumber);
            return new(JsonSerializer.Serialize(body, _settings).ToString(), Encoding.UTF8, "application/json");

        }
        else
            throw new Exception("Не удалось пройти проверку");
    }

    /// <summary>
    /// Проверка ответа
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool ValidateResponse(HttpResponseMessage? response)
    {
        //Если ответ не пустой
        if (response != null)
        {
            //Если статус ответ - Успешно, возвращаем успешный результат
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;
            //В ином случае обрабатываем ошибки
            else
            {
                //Если пришёл статус - Неавторизованн, возвращаем исключение об этом
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Некорректный токен");
                //Иначе возвращаем общее исключение
                else
                    throw new Exception("Ошибка сервера");
            }
        }
        //Иначе возвращаем общее исключение
        else
            throw new Exception("Пустой ответ");
    }

    /// <summary>
    /// Проверка данных ответа
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public bool ValidateData(BaseResponse? response)
    {
        //Если ответ не пустой
        if (response != null)
        {
            //Если статус ответ - Успешно, возвращаем успешный результат
            if (response.Success)
                return true;
            //В ином случае обрабатываем ошибки
            else
            {
                //Если есть текст ошибки, возвращаем исключение об этом
                if (response.Error != null && !string.IsNullOrEmpty(response.Error.Message))
                    throw new Exception(response.Error.Message);
                //Иначе возвращаем общее исключение
                else
                    throw new Exception("Ошибка сервера");
            }
        }
        //Иначе возвращаем общее исключение
        else
            throw new Exception("Пустой ответ");
    }

    /// <summary>
    /// Обработчик
    /// </summary>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="typeId"></param>
    /// <param name="ordinalNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<BaseResponse> Handler(string? title, string? introduction, long? typeId, long? ordinalNumber)
    {
        //Получаем строку запроса
        string url = BuilderUrl();

        //Формируем клиента и добавляем токен
        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue("Token"));

        //Формируем тело запроса
        var body = BuilderBody(title, introduction, typeId, ordinalNumber);

        //Получаем данные по запросу
        using var result = await client.PostAsync(url, body);

        if (ValidateResponse(result))
        {
            //Десериализуем ответ
            var content = await result.Content.ReadAsStringAsync();
            var respose = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

            if (ValidateData(respose))
            {
                return respose!;
            }
            else
                throw new Exception("Не пройдена проверка данных");
        }
        else
            throw new Exception("Не пройдена проверка ответа");
    }
}