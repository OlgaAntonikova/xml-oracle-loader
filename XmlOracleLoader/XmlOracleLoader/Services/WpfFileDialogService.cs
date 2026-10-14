using Microsoft.Win32;
using XmlOracleLoader.ViewModels;

namespace XmlOracleLoader.Services
{
    internal sealed class WpfFileDialogService : IFileDialogService
    {
        private const string XmlFilter = "XML files (*.xml)|*.xml";

        public string? OpenXmlFile()
        {
            var dialog = new OpenFileDialog { Filter = XmlFilter, Title = "Open 1C export" };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string? SaveXmlFile(string suggestedFileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = XmlFilter,
                Title = "Export order lines",
                FileName = suggestedFileName
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
