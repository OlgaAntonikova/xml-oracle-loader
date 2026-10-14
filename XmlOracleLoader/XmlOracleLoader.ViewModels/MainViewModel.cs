using System.Collections.ObjectModel;
using System.Xml.Linq;
using XmlOracleLoader.Core.Data;
using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Services;

namespace XmlOracleLoader.ViewModels
{
    /// <summary>
    /// Open a 1C XML export, review and validate it in a grid, import it into Oracle in one
    /// transaction, or export the Oracle table back to XML. Every step is written to the log.
    /// </summary>
    public sealed class MainViewModel : ObservableObject
    {
        private readonly IFileDialogService _dialogs;
        private readonly Func<IOrderLineRepository> _repositoryFactory;
        private readonly Func<string, XDocument> _loadXml;
        private readonly Func<DateTime> _now;
        private readonly OrderLineMapper _mapper = new();

        private string _filePath = "";
        private string _summary = "Open an XML file exported from 1C to preview it.";
        private bool _isBusy;
        private bool _imported;

        public MainViewModel(
            IFileDialogService dialogs,
            Func<IOrderLineRepository> repositoryFactory,
            Func<string, XDocument>? loadXml = null,
            Func<DateTime>? now = null)
        {
            _dialogs = dialogs;
            _repositoryFactory = repositoryFactory;
            _loadXml = loadXml ?? XDocument.Load;
            _now = now ?? (() => DateTime.Now);

            OpenFileCommand = new AsyncRelayCommand(OpenFileAsync, () => !IsBusy);
            ImportCommand = new AsyncRelayCommand(ImportAsync, () => !IsBusy && CanImport);
            ExportCommand = new AsyncRelayCommand(ExportAsync, () => !IsBusy);
        }

        public ObservableCollection<OrderLineRowViewModel> Rows { get; } = new();
        public ObservableCollection<LogEntry> Log { get; } = new();

        public AsyncRelayCommand OpenFileCommand { get; }
        public AsyncRelayCommand ImportCommand { get; }
        public AsyncRelayCommand ExportCommand { get; }

        public string FilePath
        {
            get => _filePath;
            private set
            {
                if (SetProperty(ref _filePath, value))
                    OnPropertyChanged(nameof(FileName));
            }
        }

        public string FileName => Path.GetFileName(FilePath);

        public string Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    RefreshCommands();
            }
        }

        public int InvalidRowCount => Rows.Count(r => !r.IsValid);

        /// <summary>
        /// Import is allowed only when there are rows, every row is valid and they have not
        /// been imported since the file was opened. This prevents an accidental second click;
        /// the database itself does not reject duplicate rows.
        /// </summary>
        public bool CanImport => Rows.Count > 0 && InvalidRowCount == 0 && !_imported;

        private Task OpenFileAsync()
        {
            var path = _dialogs.OpenXmlFile();
            if (path != null)
                Open(path);

            return Task.CompletedTask;
        }

        /// <summary>Loads, validates and previews a file, e.g. one passed on the command line.</summary>
        public void Open(string path)
        {
            Rows.Clear();
            _imported = false;
            FilePath = path;

            try
            {
                var parser = new XmlValueTableParser();
                var items = parser.Parse(_loadXml(path));

                var missing = _mapper.FindMissingColumns(parser.ColumnNames);
                if (missing.Count > 0)
                {
                    Summary = "The file cannot be imported.";
                    Write(LogLevel.Error, $"Required columns are missing: {string.Join(", ", missing)}");
                    return;
                }

                foreach (var row in _mapper.Map(items))
                    Rows.Add(new OrderLineRowViewModel(row));

                var invalid = InvalidRowCount;
                Summary = invalid == 0
                    ? $"{Rows.Count} rows, all valid. Ready to import."
                    : $"{Rows.Count} rows, {invalid} invalid. Fix the file before importing.";

                Write(LogLevel.Info, $"Opened {Path.GetFileName(path)}: {Rows.Count} rows.");
                foreach (var row in Rows.Where(r => !r.IsValid))
                    Write(LogLevel.Warning, $"Row {row.RowNumber}: {row.Errors}");
            }
            catch (Exception ex)
            {
                Summary = "The file cannot be read.";
                Write(LogLevel.Error, $"Cannot read {Path.GetFileName(path)}: {ex.Message}");
            }
            finally
            {
                OnPropertyChanged(nameof(InvalidRowCount));
                OnPropertyChanged(nameof(CanImport));
                RefreshCommands();
            }
        }

        private async Task ImportAsync()
        {
            IsBusy = true;
            try
            {
                var lines = Rows.Select(r => r.Line).ToList();
                Write(LogLevel.Info, $"Importing {lines.Count} rows into Oracle...");

                var inserted = await _repositoryFactory().InsertAsync(lines);

                _imported = true;
                OnPropertyChanged(nameof(CanImport));
                Write(LogLevel.Success, $"Imported {inserted} rows in one transaction.");
                Summary = $"Imported {inserted} rows.";
            }
            catch (Exception ex)
            {
                Write(LogLevel.Error, $"Import failed, nothing was saved: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExportAsync()
        {
            IsBusy = true;
            try
            {
                var path = _dialogs.SaveXmlFile($"order-lines_{_now():yyyyMMdd_HHmmss}.xml");
                if (path == null)
                    return;

                var lines = await _repositoryFactory().GetAllAsync();
                new OrderLineXmlExporter().Build(lines).Save(path);

                Write(LogLevel.Success, $"Exported {lines.Count} rows to {Path.GetFileName(path)}.");
            }
            catch (Exception ex)
            {
                Write(LogLevel.Error, $"Export failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Write(LogLevel level, string message) =>
            Log.Add(new LogEntry(_now(), level, message));

        private void RefreshCommands()
        {
            OpenFileCommand.RaiseCanExecuteChanged();
            ImportCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
        }
    }
}
