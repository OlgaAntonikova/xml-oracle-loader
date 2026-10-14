using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.Tests
{
    public class OrderLineMapperTests
    {
        private readonly OrderLineMapper _mapper = new();

        private static ItemNode Row(params (string Column, string Value)[] values) =>
            new() { Properties = values.ToDictionary(v => v.Column, v => v.Value) };

        // Every column has its own distinct value, so a swapped mapping cannot go unnoticed
        private static ItemNode FullRow(string quantity = "12.5") => Row(
            (OneCColumns.OrderNumber, "ORD-001"),
            (OneCColumns.ShipmentDate, "2026-10-20"),
            (OneCColumns.Quantity, quantity),
            (OneCColumns.ProductCode, "PRD-100"),
            (OneCColumns.ProductName, "Climbing rope 10 mm"),
            (OneCColumns.QuantityPerProduct, "2"),
            (OneCColumns.MaterialCode, "MAT-7"),
            (OneCColumns.MaterialName, "Polyamide yarn"),
            (OneCColumns.UnitOfMeasure, "m"),
            (OneCColumns.RopeLength, "60"),
            (OneCColumns.Route, "Braiding > Dyeing > Packing"),
            (OneCColumns.ProductGroup, "Ropes"),
            (OneCColumns.ProductClass, "Dynamic"),
            (OneCColumns.IsPackaging, "No"),
            (OneCColumns.ShippedQuantity, "7.25"),
            (OneCColumns.ProductionTask, "PT-2026-42"));

        private static readonly OrderLine ExpectedLine = new()
        {
            OrderNumber = "ORD-001",
            ShipmentDate = "2026-10-20",
            Quantity = 12.5m,
            ProductCode = "PRD-100",
            ProductName = "Climbing rope 10 mm",
            QuantityPerProduct = 2m,
            MaterialCode = "MAT-7",
            MaterialName = "Polyamide yarn",
            UnitOfMeasure = "m",
            RopeLength = "60",
            Route = "Braiding > Dyeing > Packing",
            ProductGroup = "Ropes",
            ProductClass = "Dynamic",
            IsPackaging = "No",
            ShippedQuantity = 7.25m,
            ProductionTask = "PT-2026-42"
        };

        [Fact]
        public void Map_MapsEveryColumnToItsProperty()
        {
            var row = Assert.Single(_mapper.Map(new[] { FullRow() }));

            Assert.True(row.IsValid, string.Join("; ", row.Errors));
            Assert.Equal(1, row.RowNumber);
            Assert.Equal(ExpectedLine, row.Line);
        }

        [Fact]
        public void Map_TreatsMissingOptionalColumnsAsEmpty()
        {
            var minimal = Row(
                (OneCColumns.OrderNumber, "ORD-002"),
                (OneCColumns.ProductCode, "PRD-200"),
                (OneCColumns.Quantity, "3"));

            var row = Assert.Single(_mapper.Map(new[] { minimal }));

            Assert.True(row.IsValid);
            Assert.Equal("", row.Line.Route);
            Assert.Equal(0m, row.Line.ShippedQuantity);
        }

        [Theory]
        [InlineData("12,5", 12.5)]
        [InlineData("1 200.75", 1200.75)]
        [InlineData("1\u00A0200,75", 1200.75)]
        [InlineData("1,200", 1.2)] // a lone comma is a decimal separator, as in 1C exports
        [InlineData("0", 0)]
        public void Map_ParsesNumbers(string quantity, double expected)
        {
            var row = Assert.Single(_mapper.Map(new[] { FullRow(quantity) }));

            Assert.True(row.IsValid, string.Join("; ", row.Errors));
            Assert.Equal((decimal)expected, row.Line.Quantity);
        }

        [Fact]
        public void Map_ReportsEveryProblemInRow()
        {
            var bad = Row(
                (OneCColumns.OrderNumber, " "),
                (OneCColumns.Quantity, "ten"),
                (OneCColumns.ShippedQuantity, "-3"));

            var row = Assert.Single(_mapper.Map(new[] { FullRow(), bad }).Skip(1));

            Assert.False(row.IsValid);
            Assert.Equal(2, row.RowNumber);
            Assert.Equal(4, row.Errors.Count);
            Assert.Contains(row.Errors, e => e.Contains(OneCColumns.OrderNumber));
            Assert.Contains(row.Errors, e => e.Contains(OneCColumns.ProductCode));
            Assert.Contains(row.Errors, e => e.Contains("not a number: 'ten'"));
            Assert.Contains(row.Errors, e => e.Contains("negative"));
        }

        [Fact]
        public void FindMissingColumns_ReturnsOnlyAbsentRequiredColumns()
        {
            var missing = _mapper.FindMissingColumns(new[] { OneCColumns.OrderNumber, "Other" });

            Assert.Equal(new[] { OneCColumns.ProductCode, OneCColumns.Quantity }, missing);
        }

        [Fact]
        public void ToItemNode_WritesEveryPropertyUnderItsColumn()
        {
            var node = _mapper.ToItemNode(ExpectedLine);

            Assert.Equal(OneCColumns.All, node.Properties.Keys);
            Assert.Equal(new Dictionary<string, string>
            {
                [OneCColumns.OrderNumber] = "ORD-001",
                [OneCColumns.ShipmentDate] = "2026-10-20",
                [OneCColumns.Quantity] = "12.5",
                [OneCColumns.ProductCode] = "PRD-100",
                [OneCColumns.ProductName] = "Climbing rope 10 mm",
                [OneCColumns.QuantityPerProduct] = "2",
                [OneCColumns.MaterialCode] = "MAT-7",
                [OneCColumns.MaterialName] = "Polyamide yarn",
                [OneCColumns.UnitOfMeasure] = "m",
                [OneCColumns.RopeLength] = "60",
                [OneCColumns.Route] = "Braiding > Dyeing > Packing",
                [OneCColumns.ProductGroup] = "Ropes",
                [OneCColumns.ProductClass] = "Dynamic",
                [OneCColumns.IsPackaging] = "No",
                [OneCColumns.ShippedQuantity] = "7.25",
                [OneCColumns.ProductionTask] = "PT-2026-42"
            }, node.Properties);
        }
    }
}
