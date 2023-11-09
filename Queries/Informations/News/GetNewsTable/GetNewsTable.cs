using Domain.Models.Base;
using Domain.Models.Informations.News.Response;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Queries.Informations.News.GetNewsTable;

/// <summary>
/// Получение списка новостей для таблицы
/// </summary>
public class GetNewsTable : IGetNewsTable
{
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private ConfigurationFile _configuration; //класс конфигурации

    /// <summary>
    /// Конструктор получения списка новостей для таблицы
    /// </summary>
    public GetNewsTable()
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
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string BuilderUrl(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted)
    {
        //Проверяем конфигурацию файла
        if (ValidateConfiguration())
        {
            //Формируем ссылку запроса
            string url = _configuration.GetValue("DefaultConnection") + _configuration.GetValue("Api")
                + _configuration.GetValue("News") + CreateQueryString(search, skip, take, sort, isDeleted);

            //Возвращаем результат
            return url;
        }
        else
            throw new Exception("Не удалось пройти проверку");
    }

    /// <summary>
    /// Формирование параметров строки
    /// </summary>
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    public string CreateQueryString(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted)
    {
        //Формируем ссылку
        string url = string.Format("table?isDeleted={0}", (isDeleted ?? true));

        //Если есть строка поиска добавляем в ссылку
        if (!String.IsNullOrEmpty(search))
            url += string.Format("&search={0}", search); ;

        //Если указано количество пропущенных элементов, добавляем
        if (skip != null)
            url += string.Format("&skip={0}", skip);

        //Если указано количество формируемых элементов, добавляем
        if (take != null)
            url += string.Format("&take={0}", take);

        //Если есть поля сортировки
        if (sort != null && sort.Any())
        {
            //Проходим по всем полям сортировки
            for (int i = 0; i < sort.Count; i++)
            {
                //Добавляем ключ сортировки
                url += string.Format("&sort[{0}].SortKey={1}", i, sort[i].SortKey);
                //Добавляем порядок сортировки
                url += string.Format("&sort[{0}].IsAscending={1}", i, sort[i].IsAscending);
            }
        }

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
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="Exception"></exception>
    public bool ValidateData(GetNewsTableResponse? response)
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
    /// <param name="search"></param>
    /// <param name="skip"></param>
    /// <param name="take"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="isDeleted"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<GetNewsTableResponse> Handler(string? search, int? skip, int? take, List<BaseSortRequest>? sort, bool? isDeleted)
    {
        //Получаем строку запроса
        string url = BuilderUrl(search, skip, take, sort, isDeleted);

        //Формируем клиента и добавляем токен
        using HttpClient client = new();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.GetValue("Token"));

        //Получаем данные по запросу
        using var result = await client.GetAsync(url);

        if (ValidateResponse(result))
        {
            //Десериализуем ответ
            var content = await result.Content.ReadAsStringAsync();
            var respose = JsonSerializer.Deserialize<GetNewsTableResponse>(content, _settings);

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