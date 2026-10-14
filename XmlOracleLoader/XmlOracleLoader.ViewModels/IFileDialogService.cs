namespace XmlOracleLoader.ViewModels
{
    /// <summary>File pickers, abstracted so the view model can be tested without WPF.</summary>
    public interface IFileDialogService
    {
        /// <returns>The chosen path, or null if the user cancelled.</returns>
        string? OpenXmlFile();

        /// <returns>The chosen path, or null if the user cancelled.</returns>
        string? SaveXmlFile(string suggestedFileName);
    }
}
