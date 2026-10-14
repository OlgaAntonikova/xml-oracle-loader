namespace XmlOracleLoader.ViewModels
{
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public sealed record LogEntry(DateTime Time, LogLevel Level, string Message)
    {
        public string TimeText => Time.ToString("HH:mm:ss");
    }
}
