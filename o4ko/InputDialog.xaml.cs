using System.Windows;

namespace o4ko
{
    public partial class InputDialog : Window
    {
        public string ResponseText => ResponseTextBox.Text;

        public InputDialog(string message, string title)
        {
            InitializeComponent();
            Title = title;
            Message = message;
            DataContext = this;
        }

        public string Message { get; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
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