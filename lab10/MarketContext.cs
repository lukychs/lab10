using Microsoft.EntityFrameworkCore; // Подключение библиотеки Entity Framework Core для работы с базой данных
using Models; // Подключение пространства имен с моделями данных (например, Ticker, Price, TodaysCondition)
using System; // Подключение стандартной библиотеки .NET
using System.Collections.Generic; // Подключение коллекций (например, List)
using System.Diagnostics;
using System.Linq; // Подключение методов LINQ для работы с запросами
using System.Text; // Подключение классов для работы с текстом
using System.Threading.Tasks; // Подключение асинхронных задач (Task, async/await)
using static Microsoft.EntityFrameworkCore.DbContextOptionsBuilder;

namespace marketContext // Пространство имен для контекста базы данных
{
    public class MarketContext : DbContext // Класс контекста базы данных, наследуемый от DbContext
    {
        // Определение таблицы Tickers в базе данных
        public DbSet<Ticker> Tickers => Set<Ticker>();

        // Определение таблицы Prices в базе данных
        public DbSet<Price> Prices => Set<Price>();

        // Определение таблицы todaysConditions в базе данных
        public DbSet<TodaysCondition> todaysConditions => Set<TodaysCondition>();

        // Конструктор контекста, который проверяет и создает базу данных, если она отсутствует
        public MarketContext() => Database.EnsureCreated();

        // Настройка конфигурации базы данных
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Указание строки подключения к базе данных SQL Server
            optionsBuilder.UseSqlServer("Server=localhost;Database=MarketDB;User Id=sa;Password=Password123!;TrustServerCertificate=True;");
        }
    }
}