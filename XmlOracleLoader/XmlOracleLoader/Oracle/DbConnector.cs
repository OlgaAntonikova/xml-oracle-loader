using System.Configuration;

namespace XmlOracleLoader.Oracle
{
  internal  abstract class DbConnector
    {
        //Connection string from App.config
        private const string connString =
            @"Data Source=(DESCRIPTION =
          (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {1}))
          (CONNECT_DATA =
           (SERVER = DEDICATED)
           (SERVICE_NAME = {2})
          )
         );
  User Id={3};Password={4}";

        protected string ConnectionString { get; }

        internal DbConnector(string ipAddress)
        {
            ConnectionString = string.Format(connString, ipAddress,
                ConfigurationManager.AppSettings["DbPort"],
                ConfigurationManager.AppSettings["DbServiceName"],
                ConfigurationManager.AppSettings["DbUserId"],
                ConfigurationManager.AppSettings["DbPwd"]);
        }
    }
}
