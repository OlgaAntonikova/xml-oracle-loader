namespace XmlOracleLoader.Core.Mapping
{
    /// <summary>
    /// Column names used by the 1C:Enterprise ValueTable export. They are part of the input
    /// file format (1C configurations name columns in Russian), so they stay as data here
    /// and are mapped to English property names in <see cref="OrderLineMapper"/>.
    /// </summary>
    public static class OneCColumns
    {
        public const string OrderNumber = "НомерЗаказа";
        public const string ShipmentDate = "ДатаОтгрузки";
        public const string Quantity = "Количество";
        public const string ProductCode = "ОбозначениеПродукции";
        public const string ProductName = "НаименованиеПродукции";
        public const string QuantityPerProduct = "КоличествоНаИзделие";
        public const string MaterialCode = "Обозначение";
        public const string MaterialName = "Наименование";
        public const string UnitOfMeasure = "ЕдиницаИзмерения";
        public const string RopeLength = "ДлинаВеревки";
        public const string Route = "МаршрутДвижения";
        public const string ProductGroup = "ГруппаИзделия";
        public const string ProductClass = "КлассИзделия";
        public const string IsPackaging = "ЭтоУпаковка";
        public const string ShippedQuantity = "КоличествоОтгрузка";
        public const string ProductionTask = "УкрупненноеПроизводственноеЗадание";

        /// <summary>All columns in the order the export writes them.</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            OrderNumber, ShipmentDate, Quantity, ProductCode, ProductName, QuantityPerProduct,
            MaterialCode, MaterialName, UnitOfMeasure, RopeLength, Route, ProductGroup,
            ProductClass, IsPackaging, ShippedQuantity, ProductionTask
        };
    }
}
