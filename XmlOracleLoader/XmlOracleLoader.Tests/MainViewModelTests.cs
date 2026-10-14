using System.Xml.Linq;
using XmlOracleLoader.Core.Data;
using XmlOracleLoader.Core.Mapping;
using XmlOracleLoader.Core.Models;
using XmlOracleLoader.ViewModels;

namespace XmlOracleLoader.Tests
{
    public class MainViewModelTests
    {
        private const string Ns = "http://v8.1c.ru/8.1/data/core";

        private sealed class FakeDialogs : IFileDialogService
        {
            public string? OpenPath { get; set; } = "orders.xml";
            public string? SavePath { get; set; }
            public string? OpenXmlFile() => OpenPath;
            public string? SaveXmlFile(string suggestedFileName) => SavePath;
        }

        private sealed class FakeRepository : IOrderLineRepository
        {
            public List<OrderLine> Stored { get; } = new();
            public Exception? Failure { get; set; }

            public Task<int> InsertAsync(IReadOnlyList<OrderLine> lines, CancellationToken cancellationToken = default)
            {
                if (Failure != null)
                    throw Failure;
                Stored.AddRange(lines);
                return Task.FromResult(lines.Count);
            }

            public Task<IReadOnlyList<OrderLine>> GetAllAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult<IReadOnlyList<OrderLine>>(Stored);
        }

        private static XDocument ValueTable(params string[][] rows)
        {
            XNamespace ns = Ns;
            var columns = new[] { OneCColumns.OrderNumber, OneCColumns.ProductCode, OneCColumns.Quantity };
            return new XDocument(new XElement(ns + "ValueTable",
                columns.Select(c => new XElement(ns + "column", new XElement(ns + "Name", c))),
                rows.Select(r => new XElement(ns + "row", r.Select(v => new XElement(ns + "Value", v))))));
        }

        private readonly FakeDialogs _dialogs = new();
        private readonly FakeRepository _repository = new();

        private MainViewModel CreateViewModel(XDocument document) =>
            new(_dialogs, () => _repository, _ => document, () => new DateTime(2026, 10, 14, 12, 0, 0));

        [Fact]
        public async Task OpenFile_ShowsRowsAndEnablesImport_WhenAllRowsAreValid()
        {
            var vm = CreateViewModel(ValueTable(new[] { "ORD-1", "PRD-1", "5" }, new[] { "ORD-1", "PRD-2", "2.5" }));

            await vm.OpenFileCommand.ExecuteAsync();

            Assert.Equal(2, vm.Rows.Count);
            Assert.True(vm.CanImport);
            Assert.True(vm.ImportCommand.CanExecute(null));
            Assert.Equal("orders.xml", vm.FilePath);
            Assert.Contains("all valid", vm.Summary);
        }

        [Fact]
        public async Task OpenFile_FlagsInvalidRowsAndBlocksImport()
        {
            var vm = CreateViewModel(ValueTable(new[] { "ORD-1", "PRD-1", "5" }, new[] { "ORD-1", "", "abc" }));

            await vm.OpenFileCommand.ExecuteAsync();

            Assert.Equal(1, vm.InvalidRowCount);
            Assert.False(vm.CanImport);
            Assert.False(vm.ImportCommand.CanExecute(null));
            Assert.Contains(vm.Log, e => e.Level == LogLevel.Warning && e.Message.StartsWith("Row 2:"));
        }

        [Fact]
        public async Task OpenFile_ReportsMissingColumns()
        {
            XNamespace ns = Ns;
            var doc = new XDocument(new XElement(ns + "ValueTable",
                new XElement(ns + "column", new XElement(ns + "Name", "Something"))));
            var vm = CreateViewModel(doc);

            await vm.OpenFileCommand.ExecuteAsync();

            Assert.Empty(vm.Rows);
            Assert.False(vm.CanImport);
            Assert.Contains(vm.Log, e => e.Level == LogLevel.Error && e.Message.Contains(OneCColumns.ProductCode));
        }

        [Fact]
        public async Task OpenFile_LogsUnreadableFileInsteadOfThrowing()
        {
            var vm = new MainViewModel(_dialogs, () => _repository, _ => throw new System.Xml.XmlException("bad xml"));

            await vm.OpenFileCommand.ExecuteAsync();

            Assert.Contains(vm.Log, e => e.Level == LogLevel.Error && e.Message.Contains("bad xml"));
        }

        [Fact]
        public async Task OpenFile_DoesNothing_WhenDialogCancelled()
        {
            _dialogs.OpenPath = null;
            var vm = CreateViewModel(ValueTable(new[] { "ORD-1", "PRD-1", "5" }));

            await vm.OpenFileCommand.ExecuteAsync();

            Assert.Empty(vm.Rows);
            Assert.Empty(vm.Log);
        }

        [Fact]
        public async Task Import_SavesAllRowsOnce()
        {
            var vm = CreateViewModel(ValueTable(new[] { "ORD-1", "PRD-1", "5" }, new[] { "ORD-1", "PRD-2", "1" }));
            await vm.OpenFileCommand.ExecuteAsync();

            await vm.ImportCommand.ExecuteAsync();
            await vm.ImportCommand.ExecuteAsync();

            Assert.Equal(new[] { "PRD-1", "PRD-2" }, _repository.Stored.Select(l => l.ProductCode));
            Assert.False(vm.CanImport);
            Assert.Contains(vm.Log, e => e.Level == LogLevel.Success && e.Message.Contains("Imported 2 rows"));
        }

        [Fact]
        public async Task Import_LogsDatabaseErrorAndAllowsRetry()
        {
            _repository.Failure = new InvalidOperationException("ORA-12541: no listener");
            var vm = CreateViewModel(ValueTable(new[] { "ORD-1", "PRD-1", "5" }));
            await vm.OpenFileCommand.ExecuteAsync();

            await vm.ImportCommand.ExecuteAsync();

            Assert.Contains(vm.Log, e => e.Level == LogLevel.Error && e.Message.Contains("ORA-12541"));
            Assert.False(vm.IsBusy);
            Assert.True(vm.ImportCommand.CanExecute(null));
        }

        private sealed class SlowRepository : IOrderLineRepository
        {
            public TaskCompletionSource<int> Release { get; } = new();
            public int Calls { get; private set; }

            public Task<int> InsertAsync(IReadOnlyList<OrderLine> lines, CancellationToken cancellationToken = default)
            {
                Calls++;
                return Release.Task;
            }

            public Task<IReadOnlyList<OrderLine>> GetAllAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult<IReadOnlyList<OrderLine>>(Array.Empty<OrderLine>());
        }

        [Fact]
        public async Task Import_IsDisabledWhileRunning_SoSecondClickDoesNothing()
        {
            var slow = new SlowRepository();
            var vm = new MainViewModel(_dialogs, () => slow, _ => ValueTable(new[] { "ORD-1", "PRD-1", "5" }));
            await vm.OpenFileCommand.ExecuteAsync();

            var firstClick = vm.ImportCommand.ExecuteAsync();
            Assert.True(vm.IsBusy);
            Assert.False(vm.ImportCommand.CanExecute(null));
            await vm.ImportCommand.ExecuteAsync();

            slow.Release.SetResult(1);
            await firstClick;

            Assert.Equal(1, slow.Calls);
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public async Task Import_LogsMissingDatabaseSettings_InsteadOfCrashing()
        {
            var vm = new MainViewModel(_dialogs,
                () => throw new InvalidOperationException("Database connection is not configured. Set: XOL_DB_USER"),
                _ => ValueTable(new[] { "ORD-1", "PRD-1", "5" }));
            await vm.OpenFileCommand.ExecuteAsync();

            await vm.ImportCommand.ExecuteAsync();

            Assert.Contains(vm.Log, e => e.Level == LogLevel.Error && e.Message.Contains("XOL_DB_USER"));
            Assert.False(vm.IsBusy);
        }

        [Fact]
        public async Task Export_WritesStoredLinesToChosenFile()
        {
            var path = Path.Combine(Path.GetTempPath(), $"xol-export-{Guid.NewGuid():N}.xml");
            _dialogs.SavePath = path;
            _repository.Stored.Add(new OrderLine { OrderNumber = "ORD-9", ProductCode = "PRD-9", Quantity = 3m });
            var vm = CreateViewModel(ValueTable());

            try
            {
                await vm.ExportCommand.ExecuteAsync();

                var item = Assert.Single(XDocument.Load(path).Root!.Elements("Item"));
                Assert.Equal("PRD-9", item.Element(OneCColumns.ProductCode)?.Value);
                Assert.Contains(vm.Log, e => e.Level == LogLevel.Success && e.Message.Contains("Exported 1 rows"));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
