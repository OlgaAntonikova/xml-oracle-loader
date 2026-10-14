using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Services;
using XmlOracleLoader.Oracle;

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

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedType = (DataTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Select data type.");
                return;
            }

            var dialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (dialog.ShowDialog() != true)
                return;

            var settings = AppDbSettings.TryLoad();
            if (settings == null)
                return;

            try
            {
                var parser = new XmlValueTableParser();
                var items = parser.Parse(XDocument.Load(dialog.FileName));

                var mapper = new OrderLineMapper();
                var missingColumns = mapper.FindMissingColumns(parser.ColumnNames);
                if (missingColumns.Count > 0)
                {
                    MessageBox.Show("The file has no required columns: " + string.Join(", ", missingColumns),
                        "Import", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rows = mapper.Map(items);
                var invalid = rows.Where(r => !r.IsValid).ToList();
                if (invalid.Count > 0)
                {
                    var details = string.Join("\n", invalid.Take(10)
                        .Select(r => $"Row {r.RowNumber}: {string.Join("; ", r.Errors)}"));
                    MessageBox.Show($"{invalid.Count} row(s) are invalid, nothing was imported.\n\n{details}",
                        "Import", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var repository = new OracleOrderLineRepository(settings);
                var inserted = await repository.InsertAsync(rows.Select(r => r.Line).ToList());

                MessageBox.Show($"{inserted} row(s) saved to the database.", $"Import {selectedType.ToLowerInvariant()} completed",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
