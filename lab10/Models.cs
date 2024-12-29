using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    // Класс, представляющий тикер (финансовый инструмент, например, акции)
    public class Ticker
    {
        public int id { get; set; } // Уникальный идентификатор тикера в базе данных

        [Column("ticker")] // Указывает, что это поле будет отображаться в базе данных как "ticker"
        public string tickerSym { get; set; } // Символ тикера (например, AAPL для Apple)

        public List<Price> prices { get; set; } // Список цен, связанных с этим тикером
    }

    // Класс, представляющий цену актива в определенный момент времени
    public class Price
    {
        public int id { get; set; } // Уникальный идентификатор записи цены в базе данных

        public int tickerId { get; set; } // Идентификатор тикера, к которому относится эта цена (внешний ключ)

        public Ticker tickerSymPrices { get; set; } // Навигационное свойство для связи с тикером

        public double price { get; set; } // Значение цены актива

        public DateTime date { get; set; } // Дата и время, к которым относится эта цена
    }

    // Класс, представляющий текущее состояние тикера
    public class TodaysCondition
    {
        public int id { get; set; } // Уникальный идентификатор записи состояния в базе данных

        public int tickerId { get; set; } // Идентификатор тикера, к которому относится это состояние (внешний ключ)

        public string state { get; set; } // Состояние тикера (например, "UP" или "DOWN")

        public Ticker tickerSymConditions { get; set; } // Навигационное свойство для связи с тикером
    }
}