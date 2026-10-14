using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.ViewModels
{
    /// <summary>One row of the preview grid.</summary>
    public sealed class OrderLineRowViewModel
    {
        public OrderLineRowViewModel(MappedRow row)
        {
            RowNumber = row.RowNumber;
            Line = row.Line;
            IsValid = row.IsValid;
            Errors = string.Join("; ", row.Errors);
        }

        public int RowNumber { get; }
        public OrderLine Line { get; }
        public bool IsValid { get; }
        public string Errors { get; }
    }
}
