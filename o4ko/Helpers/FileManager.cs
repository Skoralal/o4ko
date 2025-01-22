using Microsoft.Win32;
using System.IO;

namespace o4ko.Helpers
{
    public static class FileManager
    {
        public static string OpenImageDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public static bool FileExists(string path) => File.Exists(path);
        public static List<string> OpenMultipleImageDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                Multiselect = true
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileNames.ToList() : null;
        }

    }

}