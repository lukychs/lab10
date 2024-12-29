using Functions; // Подключение пространства имен для функций, связанных с обработкой данных
using marketContext; // Подключение пространства имен для контекста базы данных
using System; // Подключение стандартной библиотеки .NET
using System.Collections.Generic; // Подключение коллекций (например, List)
using System.Linq; // Подключение методов LINQ для работы с запросами
using System.Text; // Подключение классов для работы с текстом
using System.Threading.Tasks; // Подключение поддержки асинхронности (Task, async/await)

namespace app // Пространство имен приложения
{
    internal class App // Основной класс приложения
    {
        static async Task Main(string[] args) // Асинхронный метод Main — точка входа в приложение
        {
            using (var db = new MarketContext()) // Открытие контекста базы данных; обеспечивается автоматическое закрытие после завершения работы
            {
                var marketFuncs = new MarketFunctions(db); // Создание экземпляра класса для работы с функциями обработки данных
                Console.Write("Input ticker (e.g. AACG): "); // Просьба ввести тикер акции
                string ticker = Console.ReadLine(); // Считывание ввода пользователя

                // Определение временного интервала (от месяца назад до текущей даты)
                string startDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"); // Дата месяц назад
                string endDate = DateTime.Now.ToString("yyyy-MM-dd"); // Текущая дата
                Console.WriteLine($"Awaiting data for {ticker} from {startDate} to {endDate}..."); // Вывод информации о процессе загрузки данных

                // Асинхронное получение и сохранение данных
                await marketFuncs.GetDataAndSaveAsync(ticker, startDate, endDate);

                Console.WriteLine("Analyzing stocks..."); // Уведомление о начале анализа данных
                await marketFuncs.AnalyzeData(); // Асинхронный вызов анализа данных

                // Поиск состояния акции для заданного тикера
                var condition = db.todaysConditions
                    .FirstOrDefault(tc => tc.tickerSymConditions.tickerSym == ticker); // Поиск первого подходящего результата в таблице условий

                if (condition != null) // Если данные для тикера найдены
                {
                    Console.WriteLine($"State of stock {ticker}: {condition.state}"); // Вывод состояния акции
                }
                else // Если данных недостаточно
                {
                    Console.WriteLine($"Not enough data for ticker {ticker}"); // Сообщение о нехватке данных
                }
            } // Завершение блока using, освобождение ресурсов (закрытие базы данных)
        }
    }
}