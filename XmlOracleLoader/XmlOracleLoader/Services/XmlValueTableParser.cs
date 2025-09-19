using System.Xml.Linq;
using XmlOracleLoader.Models;

namespace XmlOracleLoader.Services
{
    public class XmlValueTableParser
    {
        public List<string> ColumnNames { get; private set; } = new List<string>();

        public List<ItemNode> Parse(string path)
        {
            XNamespace ns = "http://v8.1c.ru/8.1/data/core";
            var doc = XDocument.Load(path);
            var root = doc.Root;

            if (root?.Name.LocalName != "ValueTable")
                throw new Exception("Expected ValueTable at root");

            // Read columns
            var columnElements = root.Elements(ns + "column").ToList();
            if (columnElements.Count == 0)
                throw new Exception("Columns not found");

            ColumnNames = columnElements
                .Select(c => c.Element(ns + "Name")?.Value ?? "")
                .ToList();

            // Read rows
            var result = new List<ItemNode>();
            foreach (var rowElem in root.Elements(ns + "row"))
            {
                var node = new ItemNode();
                var values = rowElem.Elements(ns + "Value").ToList();

                for (int i = 0; i < Math.Min(values.Count, ColumnNames.Count); i++)
                {
                    var column = ColumnNames[i];
                    var value = values[i]?.Value?.Trim() ?? "";
                    node.Properties[column] = value;
                }

                result.Add(node);
            }

            return result;
        }
    }
}
