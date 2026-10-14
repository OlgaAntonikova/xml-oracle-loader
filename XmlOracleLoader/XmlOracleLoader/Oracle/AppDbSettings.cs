using System.Configuration;
using System.Windows;
using XmlOracleLoader.Core.Data;

namespace XmlOracleLoader.Oracle
{
    internal static class AppDbSettings
    {
        /// <summary>
        /// Loads connection settings from environment variables and App.config;
        /// shows what is missing instead of crashing when they are not configured.
        /// </summary>
        internal static DbSettings? TryLoad()
        {
            try
            {
                return DbSettings.Load(Environment.GetEnvironmentVariable, key => ConfigurationManager.AppSettings[key]);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
