using Domain.Models.Base;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Queries.Informations.News.GetNewsTypes;

/// <summary>
/// Получение списка типов новостей
/// </summary>
public class GetNewsTypes : IGetNewsTypes
{
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private ConfigurationFile _configuration; //класс конфигурации

    /// <summary>
    /// Получение списка типов новостей
    /// </summary>
    public GetNewsTypes()
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
    /// Формирование строки запроса
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string BuilderUrl()
    {
        //Проверяем конфигурацию файла
        if (ValidateConfiguration())
        {
            //Формируем ссылку запроса
            string url = _configuration.GetValue("DefaultConnection") + _configuration.GetValue("Api")
                + _configuration.GetValue("News") + "types/list";

            //Возвращаем результат
            return url;
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
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public bool ValidateData(BaseResponseList? response)
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
                    throw new ArgumentException("response.Error.Message");
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
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<BaseResponseList> Handler()
    {
        //Получаем строку запроса
        string url = BuilderUrl();

        //Формируем клиента и добавляем токен
        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue("Token"));

        //Получаем данные по запросу
        using var result = await client.GetAsync(url);

        if (ValidateResponse(result))
        {
            //Десериализуем ответ
            var content = await result.Content.ReadAsStringAsync();
            var respose = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

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