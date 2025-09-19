using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
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

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedType = (DataTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Select data type.");
                return;
            }

            if (selectedType == "Material map")
            {
                var items = LoadItemsFromDb();
                CreateXml(items, selectedType);
            }
            else if (selectedType == "Warehouse status")
            {
                var items = LoadItemsFromDb();
                CreateXml(items, selectedType);
            }

            // Close window
            this.Close();
        }

        // Create Xml
        private void CreateXml(List<ItemNode> items, string selectedType)
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

        // Read data from DB
        private List<ItemNode> LoadItemsFromDb()
        {
            // Read from App.config
            var ipAddress = ConfigurationManager.AppSettings["DbHost"];

            var reader = new DbReader(ipAddress);

            return reader.ReadFromDB();
        }
    }
}
