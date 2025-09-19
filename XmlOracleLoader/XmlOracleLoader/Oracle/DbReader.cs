using Oracle.ManagedDataAccess.Client;
using XmlOracleLoader.Models;

namespace XmlOracleLoader.Oracle
{
   internal class DbReader : DbConnector
    {
        public DbReader(string ipAddress) : base(ipAddress) { }

        internal List<ItemNode> ReadFromDB()
        {
            var result = new List<ItemNode>();

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM A_FROM_XML_ZAK";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ItemNode();

                            item.Properties["НомерЗаказа"] = reader["N_ZAK"]?.ToString();
                            item.Properties["ДатаОтгрузки"] = reader["DATE_OTG"]?.ToString();
                            item.Properties["Количество"] = reader["QTY"]?.ToString();
                            item.Properties["ОбозначениеПродукции"] = reader["ART"]?.ToString();
                            item.Properties["НаименованиеПродукции"] = reader["NAME"]?.ToString();
                            item.Properties["КоличествоНаИзделие"] = reader["QTY_NA_IZD"]?.ToString();
                            item.Properties["Обозначение"] = reader["ART_MAT"]?.ToString();
                            item.Properties["Наименование"] = reader["NAME_MAT"]?.ToString();
                            item.Properties["ЕдиницаИзмерения"] = reader["ED_IZM"]?.ToString();
                            item.Properties["ДлинаВеревки"] = reader["ROAT"]?.ToString();
                            item.Properties["МаршрутДвижения"] = reader["MARSHRUT"]?.ToString();
                            item.Properties["ГруппаИзделия"] = reader["GROUP_ITEM"]?.ToString();
                            item.Properties["КлассИзделия"] = reader["CLASS_ITEM"]?.ToString();
                            item.Properties["ЭтоУпаковка"] = reader["UPAK"]?.ToString();
                            item.Properties["КоличествоОтгрузка"] = reader["QTY_OTG"]?.ToString();
                            item.Properties["УкрупненноеПроизводственноеЗадание"] = reader["PVZ"]?.ToString();

                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }
    }
}
