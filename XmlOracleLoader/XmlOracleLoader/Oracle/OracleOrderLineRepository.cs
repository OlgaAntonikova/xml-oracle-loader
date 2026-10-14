using Oracle.ManagedDataAccess.Client;
using XmlOracleLoader.Core.Data;
using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.Oracle
{
    /// <summary>Reads and writes order lines in the A_FROM_XML_ZAK table.</summary>
    internal sealed class OracleOrderLineRepository : IOrderLineRepository
    {
        private const string InsertSql =
            "INSERT INTO A_FROM_XML_ZAK (N_ZAK, DATE_OTG, QTY, ART, NAME, QTY_NA_IZD, ART_MAT, NAME_MAT, " +
            "ED_IZM, ROAT, MARSHRUT, GROUP_ITEM, CLASS_ITEM, UPAK, QTY_OTG, PVZ) " +
            "VALUES (:N_ZAK, :DATE_OTG, :QTY, :ART, :NAME, :QTY_NA_IZD, :ART_MAT, :NAME_MAT, " +
            ":ED_IZM, :ROAT, :MARSHRUT, :GROUP_ITEM, :CLASS_ITEM, :UPAK, :QTY_OTG, :PVZ)";

        private const string SelectSql =
            "SELECT N_ZAK, DATE_OTG, QTY, ART, NAME, QTY_NA_IZD, ART_MAT, NAME_MAT, ED_IZM, ROAT, " +
            "MARSHRUT, GROUP_ITEM, CLASS_ITEM, UPAK, QTY_OTG, PVZ FROM A_FROM_XML_ZAK ORDER BY N_ZAK, ART, ART_MAT";

        private readonly string _connectionString;

        public OracleOrderLineRepository(DbSettings settings)
        {
            _connectionString = settings.ToConnectionString();
        }

        public async Task<int> InsertAsync(IReadOnlyList<OrderLine> lines, CancellationToken cancellationToken = default)
        {
            if (lines.Count == 0)
                return 0;

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = (OracleTransaction)await connection.BeginTransactionAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = InsertSql;
            command.BindByName = true;

            // Array binding sends all rows in a single round trip
            command.ArrayBindCount = lines.Count;

            void Bind<T>(string name, OracleDbType type, Func<OrderLine, T> value) =>
                command.Parameters.Add(name, type, lines.Select(value).ToArray(), System.Data.ParameterDirection.Input);

            Bind("N_ZAK", OracleDbType.Varchar2, l => l.OrderNumber);
            Bind("DATE_OTG", OracleDbType.Varchar2, l => l.ShipmentDate);
            Bind("QTY", OracleDbType.Decimal, l => l.Quantity);
            Bind("ART", OracleDbType.Varchar2, l => l.ProductCode);
            Bind("NAME", OracleDbType.Varchar2, l => l.ProductName);
            Bind("QTY_NA_IZD", OracleDbType.Decimal, l => l.QuantityPerProduct);
            Bind("ART_MAT", OracleDbType.Varchar2, l => l.MaterialCode);
            Bind("NAME_MAT", OracleDbType.Varchar2, l => l.MaterialName);
            Bind("ED_IZM", OracleDbType.Varchar2, l => l.UnitOfMeasure);
            Bind("ROAT", OracleDbType.Varchar2, l => l.RopeLength);
            Bind("MARSHRUT", OracleDbType.Varchar2, l => l.Route);
            Bind("GROUP_ITEM", OracleDbType.Varchar2, l => l.ProductGroup);
            Bind("CLASS_ITEM", OracleDbType.Varchar2, l => l.ProductClass);
            Bind("UPAK", OracleDbType.Varchar2, l => l.IsPackaging);
            Bind("QTY_OTG", OracleDbType.Decimal, l => l.ShippedQuantity);
            Bind("PVZ", OracleDbType.Varchar2, l => l.ProductionTask);

            // If any row fails, the transaction is disposed without Commit and Oracle rolls back
            // the rows already sent in this batch
            var inserted = await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return inserted;
        }

        public async Task<IReadOnlyList<OrderLine>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = SelectSql;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var result = new List<OrderLine>();

            string Text(int i) => reader.IsDBNull(i) ? "" : Convert.ToString(reader.GetValue(i)) ?? "";
            decimal Number(int i) => reader.IsDBNull(i) ? 0m : reader.GetDecimal(i);

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new OrderLine
                {
                    OrderNumber = Text(0),
                    ShipmentDate = Text(1),
                    Quantity = Number(2),
                    ProductCode = Text(3),
                    ProductName = Text(4),
                    QuantityPerProduct = Number(5),
                    MaterialCode = Text(6),
                    MaterialName = Text(7),
                    UnitOfMeasure = Text(8),
                    RopeLength = Text(9),
                    Route = Text(10),
                    ProductGroup = Text(11),
                    ProductClass = Text(12),
                    IsPackaging = Text(13),
                    ShippedQuantity = Number(14),
                    ProductionTask = Text(15)
                });
            }

            return result;
        }
    }
}
