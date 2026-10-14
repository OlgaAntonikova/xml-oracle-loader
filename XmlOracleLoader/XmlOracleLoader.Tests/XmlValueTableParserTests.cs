using System.Xml.Linq;
using XmlOracleLoader.Core.Services;

namespace XmlOracleLoader.Tests
{
    public class XmlValueTableParserTests
    {
        private const string Ns = "http://v8.1c.ru/8.1/data/core";

        [Fact]
        public void Parse_ReturnsRowsAndColumns()
        {
            var doc = XDocument.Parse($@"
<ValueTable xmlns=""{Ns}"">
  <column><Name>Code</Name></column>
  <column><Name>Name</Name></column>
  <column><Name>Price</Name></column>

  <row>
    <Value>P-001</Value>
    <Value>Crayon Set</Value>
    <Value>10.50</Value>
  </row>
  <row>
    <Value>P-002</Value>
    <Value>Sketchbook</Value>
    <Value>4.99</Value>
  </row>
</ValueTable>");
            var parser = new XmlValueTableParser();

            var rows = parser.Parse(doc);

            Assert.Equal(new[] { "Code", "Name", "Price" }, parser.ColumnNames);
            Assert.Equal(2, rows.Count);
            Assert.Equal("P-001", rows[0].Properties["Code"]);
            Assert.Equal("Crayon Set", rows[0].Properties["Name"]);
            Assert.Equal("10.50", rows[0].Properties["Price"]);
            Assert.Equal("P-002", rows[1].Properties["Code"]);
        }

        [Fact]
        public void Parse_IgnoresExtraValues_WhenMoreValuesThanColumns()
        {
            var doc = XDocument.Parse($@"
<ValueTable xmlns=""{Ns}"">
  <column><Name>Code</Name></column>
  <column><Name>Name</Name></column>

  <row>
    <Value>P-001</Value>
    <Value>Crayon</Value>
    <Value>EXTRA-IGNORED</Value>
  </row>
</ValueTable>");
            var parser = new XmlValueTableParser();

            var rows = parser.Parse(doc);

            var row = Assert.Single(rows);
            Assert.Equal(new[] { "Code", "Name" }, row.Properties.Keys);
            Assert.DoesNotContain("EXTRA-IGNORED", row.Properties.Values);
        }

        [Fact]
        public void Parse_Throws_WhenRootIsNotValueTable()
        {
            var doc = XDocument.Parse($@"<Root xmlns=""{Ns}""></Root>");
            var parser = new XmlValueTableParser();

            var ex = Assert.ThrowsAny<Exception>(() => parser.Parse(doc));

            Assert.Contains("ValueTable", ex.Message);
        }

        [Fact]
        public void Parse_Throws_WhenNoColumns()
        {
            var doc = XDocument.Parse($@"<ValueTable xmlns=""{Ns}""></ValueTable>");
            var parser = new XmlValueTableParser();

            var ex = Assert.ThrowsAny<Exception>(() => parser.Parse(doc));

            Assert.Contains("Columns not found", ex.Message);
        }
    }
}
