namespace XmlOracleLoader.Core.Data
{
    /// <summary>
    /// Oracle connection settings. Each value is taken from an environment variable first
    /// (XOL_DB_HOST, XOL_DB_PORT, XOL_DB_SERVICE, XOL_DB_USER, XOL_DB_PASSWORD) and falls back
    /// to the matching App.config key, so credentials never have to live in the repository.
    /// </summary>
    public sealed record DbSettings(string Host, int Port, string ServiceName, string UserId, string Password)
    {
        private static readonly (string Env, string AppKey)[] Keys =
        {
            ("XOL_DB_HOST", "DbHost"),
            ("XOL_DB_PORT", "DbPort"),
            ("XOL_DB_SERVICE", "DbServiceName"),
            ("XOL_DB_USER", "DbUserId"),
            ("XOL_DB_PASSWORD", "DbPwd")
        };

        /// <param name="environment">Reads an environment variable, e.g. Environment.GetEnvironmentVariable.</param>
        /// <param name="appSettings">Reads an App.config value, e.g. ConfigurationManager.AppSettings[key].</param>
        /// <exception cref="InvalidOperationException">A setting is missing or the port is not a number.</exception>
        public static DbSettings Load(Func<string, string?> environment, Func<string, string?> appSettings)
        {
            var values = new Dictionary<string, string>();
            var missing = new List<string>();

            foreach (var (env, appKey) in Keys)
            {
                var value = environment(env);
                if (string.IsNullOrWhiteSpace(value))
                    value = appSettings(appKey);

                if (string.IsNullOrWhiteSpace(value))
                    missing.Add($"{env} (or {appKey} in App.config)");
                else
                    values[env] = value.Trim();
            }

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    "Database connection is not configured. Set: " + string.Join(", ", missing));

            if (!int.TryParse(values["XOL_DB_PORT"], out var port) || port is <= 0 or > 65535)
                throw new InvalidOperationException($"Database port '{values["XOL_DB_PORT"]}' is not a valid port number.");

            return new DbSettings(values["XOL_DB_HOST"], port, values["XOL_DB_SERVICE"],
                values["XOL_DB_USER"], values["XOL_DB_PASSWORD"]);
        }

        public string ToConnectionString() =>
            $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Host})(PORT={Port}))" +
            $"(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={ServiceName})));" +
            $"User Id={UserId};Password={Password}";

        // Keeps the password out of logs and debugger views
        public override string ToString() =>
            $"{UserId}@{Host}:{Port}/{ServiceName}";
    }
}
