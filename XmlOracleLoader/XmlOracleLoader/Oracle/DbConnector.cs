using XmlOracleLoader.Core.Data;

namespace XmlOracleLoader.Oracle
{
    internal abstract class DbConnector
    {
        protected string ConnectionString { get; }

        internal DbConnector(DbSettings settings)
        {
            ConnectionString = settings.ToConnectionString();
        }
    }
}
