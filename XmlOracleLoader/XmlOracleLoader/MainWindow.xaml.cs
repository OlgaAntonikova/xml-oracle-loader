using System.Configuration;
using System.Windows;
using XmlOracleLoader.Core.Data;
using XmlOracleLoader.Oracle;
using XmlOracleLoader.Services;
using XmlOracleLoader.ViewModels;

namespace XmlOracleLoader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Settings are read when a database action runs, so the preview works without a
            // configured database and a configuration error shows up in the log
            var viewModel = new MainViewModel(
                new WpfFileDialogService(),
                () => new OracleOrderLineRepository(
                    DbSettings.Load(Environment.GetEnvironmentVariable, key => ConfigurationManager.AppSettings[key])));
            DataContext = viewModel;

            // XmlOracleLoader.exe path\to\export.xml opens the file right away;
            // a wrong path is reported in the log
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                viewModel.Open(args[1]);
        }
    }
}
