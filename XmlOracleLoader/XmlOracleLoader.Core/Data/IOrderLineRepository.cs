using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.Core.Data
{
    public interface IOrderLineRepository
    {
        /// <summary>Inserts all lines in one transaction: either every line is saved or none.</summary>
        /// <returns>Number of inserted rows.</returns>
        Task<int> InsertAsync(IReadOnlyList<OrderLine> lines, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrderLine>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
