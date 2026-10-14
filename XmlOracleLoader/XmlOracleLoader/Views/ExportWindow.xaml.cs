using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Models;
using XmlOracleLoader.Oracle;

namespace XmlOracleLoader.Views
{
    /// <summary>
    /// Interaction logic for ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        public ExportWindow()
        {
            InitializeComponent();
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedType = (DataTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Select data type.");
                return;
            }

            var settings = AppDbSettings.TryLoad();
            if (settings == null)
                return;

            try
            {
                var lines = await new OracleOrderLineRepository(settings).GetAllAsync();
                var mapper = new OrderLineMapper();
                CreateXml(lines.Select(mapper.ToItemNode).ToList(), selectedType);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create Xml
        private static void CreateXml(List<ItemNode> items, string selectedType)
        {
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            string fileName = $"{selectedType}_{DateTime.Now:yyyyMMdd_HHmmss}.xml";

            string filePath = Path.Combine(downloadsPath, fileName);

            var root = new XElement("Items");

            foreach (var item in items)
            {
                var element = new XElement("Item");

                foreach (var kv in item.Properties)
                {
                    element.Add(new XElement(kv.Key, kv.Value));
                }

                root.Add(element);
            }

            var doc = new XDocument(root);
            doc.Save(filePath);

            MessageBox.Show($"The file was saved successfully:\n{filePath}", "Export completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
