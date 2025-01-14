using System.Windows;
using System.Windows.Controls;

namespace o4ko.Views
{
    public partial class HighlightPropertiesDialog : Window
    {
        public string HighlightName { get; private set; }
        public string DataType { get; private set; }

        public HighlightPropertiesDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightName = HighlightNameTextBox.Text;
            DataType = (DataTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(HighlightName) || string.IsNullOrEmpty(DataType))
            {
                MessageBox.Show("Please provide both a name and a data type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}