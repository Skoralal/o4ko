using System.Windows;
using System.IO;

namespace o4ko
{
    public partial class ExportWindow : Window
    {
        public ExportWindow(string serializedData, string format)
        {
            InitializeComponent();
            ExportTextBox.Text = serializedData;
            Title = $"Export as {format.ToUpper()}";
        }

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ExportTextBox.Text);
            MessageBox.Show("Data copied to clipboard.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "All files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = Title.Contains("JSON") ? "export.json" : Title.Contains("XML") ? "export.xml" : "export.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, ExportTextBox.Text);
                MessageBox.Show("File saved successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}