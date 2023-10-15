using Client.Models.Identification.Users.Response;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services.Base;

public class BaseService : IBaseService
{
    public ILogger _logger { get { return Log.ForContext<BaseService>(); } } //логгер для записи логов
    public JsonSerializerOptions _jsonSettings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор базового класса
    /// </summary>
    public BaseService()
    {
        //Выставляем параметры десериализации
        _jsonSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async Task CheckConnection()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            try
            {
                //Если в конфиге есть данные для формирования ссылки запроса
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"]))
                {
                    //Формируем ссылку запроса
                    url = ConfigurationManager.AppSettings["DefaultConnection"];

                    //Получаем данные по запросу
                    using HttpClient client = new();

                    using var result = await client.GetAsync(url);

                    //Если получили не успешный результат
                    if (result == null || result.StatusCode != System.Net.HttpStatusCode.OK)
                        SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку");
                }
                //Иначе возвращаем ошибку
                else
                    SetError("Не указан адрес api. Обратитесь в техническую поддержку");
            }
            catch
            {
                SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку");
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    /// <summary>
    /// Метод отображения ошибок
    /// </summary>
    /// <param name="message"></param>
    public void SetError(string message)
    {
        try
        {
            //Формируем новое окно с ошибкой и отображаем его
            Message messageWindow = new(message);
            messageWindow.Show();
        }
        catch (Exception ex)
        {
            _logger.Error("BaseUserControl. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Получение информации о пользователе
    /// </summary>
    /// <returns></returns>
    public async Task<UserInfoResponse> GetUserInfo()
    {
        try
        {
            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization"]))
            {
                //Формируем ссылку запроса
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Authorization"] + "userInfo";

                //Формируем клиента и добавляем токен
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем результат запроса
                using var result = await client.GetAsync(url);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем информацию о пользователе
                    var content = await result.Content.ReadAsStringAsync();
                    var _userInfo = JsonSerializer.Deserialize<UserInfoResponse>(content, _jsonSettings);

                    return _userInfo;
                }
                else
                    throw new Exception("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку");
            }
            //Иначе возвращаем ошибку
            else
                throw new Exception("Не указаны адреса api. Обратитесь в техническую поддержку");
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Метод получения настроек json
    /// </summary>
    /// <returns></returns>
    public JsonSerializerOptions GetJsonSettings()
    {
        return _jsonSettings;
    }
}