namespace XmlOracleLoader.Core.Models
{
    /// <summary>
    /// One row of a production order export: a product, one of its materials and shipment details.
    /// Maps to the A_FROM_XML_ZAK table.
    /// </summary>
    public sealed record OrderLine
    {
        public string OrderNumber { get; init; } = "";
        public string ShipmentDate { get; init; } = "";
        public decimal Quantity { get; init; }
        public string ProductCode { get; init; } = "";
        public string ProductName { get; init; } = "";
        public decimal QuantityPerProduct { get; init; }
        public string MaterialCode { get; init; } = "";
        public string MaterialName { get; init; } = "";
        public string UnitOfMeasure { get; init; } = "";
        public string RopeLength { get; init; } = "";
        public string Route { get; init; } = "";
        public string ProductGroup { get; init; } = "";
        public string ProductClass { get; init; } = "";
        public string IsPackaging { get; init; } = "";
        public decimal ShippedQuantity { get; init; }
        public string ProductionTask { get; init; } = "";
    }
}
