using System.Xml.Linq;
using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Models;

namespace XmlOracleLoader.Core.Services
{
    /// <summary>
    /// Builds the export document: &lt;Items&gt;&lt;Item&gt; with one element per 1C column,
    /// the format the 1C side reads back.
    /// </summary>
    public sealed class OrderLineXmlExporter
    {
        private readonly OrderLineMapper _mapper = new();

        public XDocument Build(IEnumerable<OrderLine> lines)
        {
            var root = new XElement("Items");

            foreach (var line in lines)
            {
                var item = new XElement("Item");
                foreach (var (column, value) in _mapper.ToItemNode(line).Properties)
                    item.Add(new XElement(column, value));

                root.Add(item);
            }

            return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        }
    }
}
