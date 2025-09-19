using System.Windows.Input;
using XmlOracleLoader.Views;

namespace XmlOracleLoader.ViewModels
{
   public class MainViewModel
    {
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }

        public MainViewModel()
        {
            ImportCommand = new RelayCommand(OnImport);
            ExportCommand = new RelayCommand(OnExport);
        }

        private static void OnImport()
        {
            new ImportWindow().ShowDialog();

        }

        private static void OnExport()
        {
            new ExportWindow().ShowDialog();
        }
    }
}
