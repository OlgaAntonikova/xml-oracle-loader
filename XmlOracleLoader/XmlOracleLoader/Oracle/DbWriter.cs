using Oracle.ManagedDataAccess.Client;
using System.Globalization;
using XmlOracleLoader.Models;

namespace XmlOracleLoader.Oracle
{
   internal class DbWriter : DbConnector
    {
        public DbWriter(string ipAddress) : base(ipAddress) { }

        internal void WriteToDB(ItemNode item, int j)
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"INSERT INTO A_FROM_XML_ZAK(N_ZAK, DATE_OTG, QTY, ART, NAME,QTY_NA_IZD,ART_MAT, NAME_MAT,ED_IZM, ROAT, MARSHRUT, GROUP_ITEM, CLASS_ITEM, UPAK, QTY_OTG, PVZ) VALUES(:N_ZAK, :DATE_OTG, :QTY, :ART, :NAME, :QTY_NA_IZD, :ART_MAT, :NAME_MAT, :ED_IZM, :ROAT, :MARSHRUT, :GROUP_ITEM, :CLASS_ITEM, :UPAK, :QTY_OTG, :PVZ)";

                    //  Describing the types of parameters
                    cmd.Parameters.Add("N_ZAK", OracleDbType.Varchar2);
                    cmd.Parameters.Add("DATE_OTG", OracleDbType.Varchar2);
                    cmd.Parameters.Add("QTY", OracleDbType.Decimal);
                    cmd.Parameters.Add("ART", OracleDbType.Varchar2);
                    cmd.Parameters.Add("NAME", OracleDbType.Varchar2);
                    cmd.Parameters.Add("QTY_NA_IZD", OracleDbType.Decimal);
                    cmd.Parameters.Add("ART_MAT", OracleDbType.Varchar2);
                    cmd.Parameters.Add("NAME_MAT", OracleDbType.Varchar2);
                    cmd.Parameters.Add("ED_IZM", OracleDbType.Varchar2);
                    cmd.Parameters.Add("ROAT", OracleDbType.Varchar2);
                    cmd.Parameters.Add("MARSHRUT", OracleDbType.Varchar2);
                    cmd.Parameters.Add("GROUP_ITEM", OracleDbType.Varchar2);
                    cmd.Parameters.Add("CLASS_ITEM", OracleDbType.Varchar2);
                    cmd.Parameters.Add("UPAK", OracleDbType.Varchar2);
                    cmd.Parameters.Add("QTY_OTG", OracleDbType.Decimal);
                    cmd.Parameters.Add("PVZ", OracleDbType.Varchar2);

                    // Array of parameter values                  
                    cmd.Parameters["N_ZAK"].Value = item.Properties["НомерЗаказа"];
                    cmd.Parameters["DATE_OTG"].Value = item.Properties["ДатаОтгрузки"];
                    double.TryParse(item.Properties["Количество"], NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity);
                    cmd.Parameters["QTY"].Value = quantity;
                    cmd.Parameters["ART"].Value = item.Properties["ОбозначениеПродукции"];
                    cmd.Parameters["NAME"].Value = item.Properties["НаименованиеПродукции"];
                    double.TryParse(item.Properties["КоличествоНаИзделие"], NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity_izd);
                    cmd.Parameters["QTY_NA_IZD"].Value = quantity_izd;
                    cmd.Parameters["ART_MAT"].Value = item.Properties["Обозначение"];
                    cmd.Parameters["NAME_MAT"].Value = item.Properties["Наименование"];
                    cmd.Parameters["ED_IZM"].Value = item.Properties["ЕдиницаИзмерения"];
                    cmd.Parameters["ROAT"].Value = item.Properties["ДлинаВеревки"];
                    cmd.Parameters["MARSHRUT"].Value = item.Properties["МаршрутДвижения"];
                    cmd.Parameters["GROUP_ITEM"].Value = item.Properties["ГруппаИзделия"];
                    cmd.Parameters["CLASS_ITEM"].Value = item.Properties["КлассИзделия"];
                    cmd.Parameters["UPAK"].Value = item.Properties["ЭтоУпаковка"];
                    double.TryParse(item.Properties["КоличествоОтгрузка"], NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity_otgr);
                    cmd.Parameters["QTY_OTG"].Value = quantity_otgr;
                    cmd.Parameters["PVZ"].Value = item.Properties["УкрупненноеПроизводственноеЗадание"];

                    // INSERT                    
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
