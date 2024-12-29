using marketContext; // Подключение контекста базы данных MarketContext
using Models; // Подключение моделей данных (Ticker, Price, TodaysCondition)
using System; // Подключение стандартной библиотеки .NET
using System.Collections; // Подключение интерфейса коллекций
using System.Collections.Generic; // Подключение коллекций (например, List)
using System.Linq; // Подключение методов LINQ для запросов к данным
using System.Reflection;
using System.Text; // Подключение для работы с текстом
using System.Text.Json; // Подключение для работы с JSON
using System.Threading.Tasks; // Подключение асинхронных задач (Task, async/await)

namespace Functions // Пространство имен для функций работы с данными
{
    // Класс для десериализации данных акций из API
    public class StockData
    {
        public string s { get; set; } // Статус ответа (например, "OK")
        public List<double> c { get; set; } // Список цен закрытия
        public List<double> h { get; set; } // Список максимальных цен
        public List<double> l { get; set; } // Список минимальных цен
        public List<double> o { get; set; } // Список цен открытия
        public List<int> t { get; set; } // Список временных меток (UNIX)
        public List<int> v { get; set; } // Список объемов торговли
    };

    // Класс, содержащий методы для работы с рынком и базой данных
    public class MarketFunctions
    {
        private readonly MarketContext _db; // Поле для хранения контекста базы данных

        // Конструктор, принимающий контекст базы данных
        public MarketFunctions(MarketContext db)
        {
            _db = db;
        }

        // Метод для получения данных из API и сохранения их в базу данных
        public async Task GetDataAndSaveAsync(string ticker, string startDate, string endDate)
        {
            string apiKey = "bWROMnE5ZmVGVFRjclI0c01qNzNFaUVzYWNGbmc4enIwcmJ6Z0ZXZkVHbz0"; // API ключ для авторизации
            // Формирование URL запроса
            string URL = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={startDate}&to={endDate}&token={apiKey}";
            HttpClient client = new HttpClient(); // Создание HTTP клиента
            var response = await client.GetAsync(URL); // Выполнение запроса

            // Проверка успешности ответа
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Something went wrong: {response.StatusCode}"); // Вывод ошибки
                return;
            }

            var json = await response.Content.ReadAsStringAsync(); // Чтение содержимого ответа
            var data = JsonSerializer.Deserialize<StockData>(json); // Десериализация JSON данных

            // Проверка корректности данных
            if (data == null || data.s == null || data.c == null || data.h == null ||
                data.l == null || data.o == null || data.t == null || data.v == null)
            {
                Console.WriteLine($"Fetching data error: Not enough data"); // Ошибка получения данных
                return;
            }

            // Проверка наличия тикера в базе данных
            var dbTicker = _db.Tickers.FirstOrDefault(t => t.tickerSym == ticker);
            await _db.SaveChangesAsync(); // Сохранение изменений в базу данных

            if (dbTicker == null) // Если тикер отсутствует
            {
                dbTicker = new Models.Ticker { tickerSym = ticker }; // Создание нового тикера
                _db.Tickers.Add(dbTicker); // Добавление тикера в базу данных
                await _db.SaveChangesAsync(); // Сохранение изменений
            }

            // Цикл для добавления цен в базу данных
            for (int i = 0; i < data.c.Count; i++)
            {
                var price = data.c[i]; // Цена закрытия
                var time = DateTimeOffset.FromUnixTimeSeconds(data.t[i]).DateTime; // Преобразование временной метки
                _db.Prices.Add(new Models.Price // Создание новой записи цены
                {
                    price = price,
                    date = time,
                    tickerId = dbTicker.id
                });
            }
            await _db.SaveChangesAsync(); // Сохранение всех изменений
        }

        // Метод для анализа данных
        public async Task AnalyzeData()
        {
            var Tickers = _db.Tickers.ToList(); // Получение всех тикеров из базы данных

            // Цикл по всем тикерам
            foreach (var ticker in Tickers)
            {
                // Получение цен для текущего тикера
                var prices = _db.Prices
                    .Where(p => p.tickerSymPrices == ticker) // Фильтрация по тикеру
                    .OrderByDescending(p => p.date) // Сортировка по дате (от последнего к первому)
                    .ToList();

                if (prices.Count >= 2) // Если данных достаточно для анализа
                {
                    var latestPrice = prices[0]; // Последняя цена
                    var previousPrice = prices.FirstOrDefault(p => p.price != latestPrice.price); // Предыдущая отличающаяся цена

                    // Определение состояния (рост или падение)
                    if (previousPrice != null && latestPrice != null)
                    {
                        var condition = latestPrice.price > previousPrice.price
                            ? $"UP    Latest Price: {latestPrice.price}    Previous Price: {previousPrice.price}"
                            : $"Down    Latest Price: {latestPrice.price}    Previous Price: {previousPrice.price}";

                        // Добавление состояния в базу данных
                        _db.todaysConditions.Add(new TodaysCondition
                        {
                            tickerId = ticker.id,
                            state = condition
                        });
                    }
                }
            }
            await _db.SaveChangesAsync(); // Сохранение изменений в базу данных
        }
    }
}