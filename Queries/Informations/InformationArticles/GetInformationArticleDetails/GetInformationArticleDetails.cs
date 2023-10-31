using Domain.Models.Base;
using Domain.Models.Informations.InformationArticlesDetails.Response;
using Queries.Informations.InformationArticles.GetListInformationArticles;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Queries.Informations.InformationArticles.GetInformationArticleDetails;

/// <summary>
/// Получение списка информационных статей
/// </summary>
public class GetInformationArticleDetails : IGetInformationArticleDetails
{
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private ConfigurationFile _configuration; //класс конфигурации

    /// <summary>
    /// Конструктор получения списка информационных статей
    /// </summary>
    public GetInformationArticleDetails()
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

        if (string.IsNullOrEmpty(_configuration.GetValue("InformationArticles")))
            throw new Exception("Не указан адрес сервиса информационных статей");

        //Возвращаем результат
        return true;
    }

    /// <summary>
    /// Формирование строки запроса
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string BuilderUrl(long? id)
    {
        //Проверяем конфигурацию файла
        if (ValidateConfiguration())
        {
            //Формируем ссылку запроса
            string url = _configuration.GetValue("DefaultConnection") + _configuration.GetValue("Api")
                + _configuration.GetValue("InformationArticles") + "details" + CreateQueryString(id);

            //Возвращаем результат
            return url;
        }
        else
            throw new Exception("Не удалось пройти проверку");
    }

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string CreateQueryString(long? id)
    {
        //Формируем ссылку
        string url = string.Empty;

        //Если есть строка поиска добавляем в ссылку
        if (id != 0)
            url += string.Format("?id={0}", id);

        //Возвращаем результат
        return url;
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
    public bool ValidateData(GetInformationArticleDetailsResponse? response)
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
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<GetInformationArticleDetailsResponse> Handler(long? id)
    {
        //Получаем строку запроса
        string url = BuilderUrl(id);

        //Формируем клиента и добавляем токен
        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue("Token"));

        //Получаем данные по запросу
        using var result = await client.GetAsync(url);

        if (ValidateResponse(result))
        {
            //Десериализуем ответ
            var content = await result.Content.ReadAsStringAsync();
            var respose = JsonSerializer.Deserialize<GetInformationArticleDetailsResponse>(content, _settings);

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