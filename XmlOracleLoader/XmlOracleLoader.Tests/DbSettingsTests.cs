using XmlOracleLoader.Core.Data;

namespace XmlOracleLoader.Tests
{
    public class DbSettingsTests
    {
        private static readonly Dictionary<string, string> AppConfig = new()
        {
            ["DbHost"] = "localhost",
            ["DbPort"] = "1521",
            ["DbServiceName"] = "FREEPDB1",
            ["DbUserId"] = "",
            ["DbPwd"] = ""
        };

        private static Func<string, string?> From(Dictionary<string, string> values) =>
            key => values.TryGetValue(key, out var value) ? value : null;

        [Fact]
        public void Load_TakesEnvironmentVariablesBeforeAppConfig()
        {
            var env = new Dictionary<string, string>
            {
                ["XOL_DB_HOST"] = "db.example.com",
                ["XOL_DB_USER"] = "loader",
                ["XOL_DB_PASSWORD"] = "secret"
            };

            var settings = DbSettings.Load(From(env), From(AppConfig));

            Assert.Equal(new DbSettings("db.example.com", 1521, "FREEPDB1", "loader", "secret"), settings);
        }

        [Fact]
        public void Load_ListsEveryMissingSetting()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => DbSettings.Load(From(new()), From(AppConfig)));

            Assert.Contains("XOL_DB_USER", ex.Message);
            Assert.Contains("XOL_DB_PASSWORD", ex.Message);
            Assert.DoesNotContain("XOL_DB_HOST", ex.Message);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("70000")]
        public void Load_RejectsInvalidPort(string port)
        {
            var env = new Dictionary<string, string>
            {
                ["XOL_DB_PORT"] = port,
                ["XOL_DB_USER"] = "loader",
                ["XOL_DB_PASSWORD"] = "secret"
            };

            Assert.Throws<InvalidOperationException>(() => DbSettings.Load(From(env), From(AppConfig)));
        }

        [Fact]
        public void ToConnectionString_BuildsOracleDescriptor()
        {
            var settings = new DbSettings("db.example.com", 1522, "ORCL", "loader", "secret");

            Assert.Equal(
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=db.example.com)(PORT=1522))" +
                "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));User Id=loader;Password=secret",
                settings.ToConnectionString());
        }

        [Fact]
        public void ToString_DoesNotRevealPassword()
        {
            var settings = new DbSettings("db.example.com", 1521, "ORCL", "loader", "secret");

            Assert.DoesNotContain("secret", settings.ToString());
        }
    }
}
