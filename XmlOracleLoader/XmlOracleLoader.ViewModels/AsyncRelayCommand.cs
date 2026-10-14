using System.Windows.Input;

namespace XmlOracleLoader.ViewModels
{
    /// <summary>
    /// Command for an async action. It is disabled while running, so a double click cannot
    /// start the same import twice.
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isRunning;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (() => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => !_isRunning && _canExecute();

        public async void Execute(object? parameter) => await ExecuteAsync();

        /// <summary>Runs the action; exposed so tests can await it.</summary>
        public async Task ExecuteAsync()
        {
            if (!CanExecute(null))
                return;

            _isRunning = true;
            RaiseCanExecuteChanged();
            try
            {
                await _execute();
            }
            finally
            {
                _isRunning = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
