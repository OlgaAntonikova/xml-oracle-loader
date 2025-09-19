using Microsoft.Win32;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using XmlOracleLoader.Models;
using XmlOracleLoader.Oracle;
using XmlOracleLoader.Services;

namespace XmlOracleLoader.Views
{
    /// <summary>
    /// Interaction logic for ImportWindow.xaml
    /// </summary>
    public partial class ImportWindow : Window
    {
        public ImportWindow()
        {
            InitializeComponent();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedType = (DataTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Select data type.");
                return;
            }

            var dialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                if (selectedType == "Materials")
                {
                    var materials = LoadItemsFromXml(filePath);
                    SaveMaterialsToOracle(materials);
                }
                else if (selectedType == "Products")
                {
                    var products = LoadItemsFromXml(filePath);
                    SaveProductsToOracle(products);
                }
            }

            // Close window
            this.Close();
        }

        //XML Loading Methods:
        //For materials
        //For products
        //TODO: In the future it will be two different methods
        private List<ItemNode> LoadItemsFromXml(string path)
        {
            var parser = new XmlValueTableParser();
            List<ItemNode> items = parser.Parse(path);
            return items;
        }

        //Oracle Storage Methods:
        //For materials
        //TODO: In the future it will be different method
        private void SaveMaterialsToOracle(List<ItemNode> materials)
        {
            var i = 1;
            // Read App.config
            var ipAddress = ConfigurationManager.AppSettings["DbHost"];
            var writer = new DbWriter(ipAddress);

            foreach (var material in materials)
            {
                writer.WriteToDB(material, i);
                i++;
            }

            MessageBox.Show($"Data successfully saved to the database", "Import materials completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //For products
        private void SaveProductsToOracle(List<ItemNode> products)
        {
            var i = 1;
            // Read App.config
            var ipAddress = ConfigurationManager.AppSettings["DbHost"];
            var writer = new DbWriter(ipAddress);

            foreach (var product in products)
            {
                writer.WriteToDB(product, i);
                i++;
            }

            MessageBox.Show($"Data successfully saved to the database", "Import products completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
