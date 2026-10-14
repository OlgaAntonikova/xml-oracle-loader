using System.Globalization;
using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.Core.Mapping
{
    /// <summary>A parsed row with its 1-based position in the file and any validation errors.</summary>
    public sealed record MappedRow(int RowNumber, OrderLine Line, IReadOnlyList<string> Errors)
    {
        public bool IsValid => Errors.Count == 0;
    }

    /// <summary>
    /// Converts generic ValueTable rows into typed <see cref="OrderLine"/> records, validating
    /// required fields and numbers, and converts them back for the XML export.
    /// </summary>
    public sealed class OrderLineMapper
    {
        private static readonly string[] RequiredColumns =
        {
            OneCColumns.OrderNumber, OneCColumns.ProductCode, OneCColumns.Quantity
        };

        /// <summary>Required columns that the file does not contain at all.</summary>
        public IReadOnlyList<string> FindMissingColumns(IEnumerable<string> columnNames)
        {
            var present = new HashSet<string>(columnNames, StringComparer.Ordinal);
            return RequiredColumns.Where(c => !present.Contains(c)).ToList();
        }

        public IReadOnlyList<MappedRow> Map(IReadOnlyList<ItemNode> rows) =>
            rows.Select((row, index) => Map(row, index + 1)).ToList();

        private static MappedRow Map(ItemNode row, int rowNumber)
        {
            var errors = new List<string>();

            string Text(string column) =>
                row.Properties.TryGetValue(column, out var value) ? value.Trim() : "";

            string Required(string column)
            {
                var value = Text(column);
                if (value.Length == 0)
                    errors.Add($"'{column}' is empty");
                return value;
            }

            decimal Number(string column, bool required)
            {
                var value = Text(column);
                if (value.Length == 0)
                {
                    if (required)
                        errors.Add($"'{column}' is empty");
                    return 0m;
                }

                if (!TryParseNumber(value, out var number))
                {
                    errors.Add($"'{column}' is not a number: '{value}'");
                    return 0m;
                }

                if (number < 0)
                    errors.Add($"'{column}' is negative: {value}");

                return number;
            }

            var line = new OrderLine
            {
                OrderNumber = Required(OneCColumns.OrderNumber),
                ShipmentDate = Text(OneCColumns.ShipmentDate),
                Quantity = Number(OneCColumns.Quantity, required: true),
                ProductCode = Required(OneCColumns.ProductCode),
                ProductName = Text(OneCColumns.ProductName),
                QuantityPerProduct = Number(OneCColumns.QuantityPerProduct, required: false),
                MaterialCode = Text(OneCColumns.MaterialCode),
                MaterialName = Text(OneCColumns.MaterialName),
                UnitOfMeasure = Text(OneCColumns.UnitOfMeasure),
                RopeLength = Text(OneCColumns.RopeLength),
                Route = Text(OneCColumns.Route),
                ProductGroup = Text(OneCColumns.ProductGroup),
                ProductClass = Text(OneCColumns.ProductClass),
                IsPackaging = Text(OneCColumns.IsPackaging),
                ShippedQuantity = Number(OneCColumns.ShippedQuantity, required: false),
                ProductionTask = Text(OneCColumns.ProductionTask)
            };

            return new MappedRow(rowNumber, line, errors);
        }

        /// <summary>
        /// Accepts "12.5" and "12,5" with optional thousands spaces (regular or no-break).
        /// A comma is a decimal separator unless the value also contains a dot, so "1,200" is 1.2,
        /// matching how 1C writes numbers.
        /// </summary>
        private static bool TryParseNumber(string value, out decimal number)
        {
            var normalized = value.Replace(" ", "").Replace("\u00A0", "");
            if (!normalized.Contains('.'))
                normalized = normalized.Replace(',', '.');

            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out number);
        }

        /// <summary>Converts a line back to 1C column names for the XML export.</summary>
        public ItemNode ToItemNode(OrderLine line)
        {
            static string Num(decimal value) => value.ToString(CultureInfo.InvariantCulture);

            return new ItemNode
            {
                Properties = new Dictionary<string, string>
                {
                    [OneCColumns.OrderNumber] = line.OrderNumber,
                    [OneCColumns.ShipmentDate] = line.ShipmentDate,
                    [OneCColumns.Quantity] = Num(line.Quantity),
                    [OneCColumns.ProductCode] = line.ProductCode,
                    [OneCColumns.ProductName] = line.ProductName,
                    [OneCColumns.QuantityPerProduct] = Num(line.QuantityPerProduct),
                    [OneCColumns.MaterialCode] = line.MaterialCode,
                    [OneCColumns.MaterialName] = line.MaterialName,
                    [OneCColumns.UnitOfMeasure] = line.UnitOfMeasure,
                    [OneCColumns.RopeLength] = line.RopeLength,
                    [OneCColumns.Route] = line.Route,
                    [OneCColumns.ProductGroup] = line.ProductGroup,
                    [OneCColumns.ProductClass] = line.ProductClass,
                    [OneCColumns.IsPackaging] = line.IsPackaging,
                    [OneCColumns.ShippedQuantity] = Num(line.ShippedQuantity),
                    [OneCColumns.ProductionTask] = line.ProductionTask
                }
            };
        }
    }
}
