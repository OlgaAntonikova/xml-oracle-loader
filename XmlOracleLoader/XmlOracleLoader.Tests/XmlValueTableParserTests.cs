using XmlOracleLoader.Core.Services;
using FluentAssertions;
using System.Xml.Linq;

namespace XmlOracleLoader.Tests
{
    public class XmlValueTableParserTests
    {
        private const string Ns = "http://v8.1c.ru/8.1/data/core";

        private static string WriteTempXml(string xml)
        {
            var path = Path.GetTempFileName();
            File.WriteAllText(path, xml);
            return path;
        }

        [Fact]
        public void Parse_ReturnsRowsAndColumns()
        {
            var xml = $@"
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
</ValueTable>";

            var path = WriteTempXml(xml);
            var parser = new XmlValueTableParser();

            var doc = XDocument.Load(path);
            var rows = parser.Parse(doc);

            parser.ColumnNames.Should().BeEquivalentTo(new[] { "Code", "Name", "Price" });
            rows.Should().HaveCount(2);
            rows[0].Properties["Code"].Should().Be("P-001");
            rows[0].Properties["Name"].Should().Be("Crayon Set");
            rows[0].Properties["Price"].Should().Be("10.50");
            rows[1].Properties["Code"].Should().Be("P-002");
        }

        [Fact]
        public void Parse_IgnoresExtraValues_WhenMoreValuesThanColumns()
        {
            var xml = $@"
<ValueTable xmlns=""{Ns}"">
  <column><Name>Code</Name></column>
  <column><Name>Name</Name></column>

  <row>
    <Value>P-001</Value>
    <Value>Crayon</Value>
    <Value>EXTRA-IGNORED</Value>
  </row>
</ValueTable>";

            var path = WriteTempXml(xml);
            var parser = new XmlValueTableParser();

            var doc = XDocument.Load(path);
            var rows = parser.Parse(doc);

            rows.Should().HaveCount(1);
            rows[0].Properties.Should().ContainKeys("Code", "Name");
            rows[0].Properties.Should().NotContainKey("EXTRA"); 
        }

        [Fact]
        public void Parse_Throws_WhenRootIsNotValueTable()
        {
            var xml = $@"<Root xmlns=""{Ns}""></Root>";
            var path = WriteTempXml(xml);
            var parser = new XmlValueTableParser();

            var doc = XDocument.Load(path);
            Action act = () => parser.Parse(doc);

            act.Should().Throw<Exception>()
               .WithMessage("*ValueTable*");
        }

        [Fact]
        public void Parse_Throws_WhenNoColumns()
        {
            var xml = $@"<ValueTable xmlns=""{Ns}""></ValueTable>";
            var path = WriteTempXml(xml);
            var parser = new XmlValueTableParser();

            var doc = XDocument.Load(path);
            Action act = () => parser.Parse(doc);

            act.Should().Throw<Exception>()
               .WithMessage("*Columns not found*");
        }
    }
}