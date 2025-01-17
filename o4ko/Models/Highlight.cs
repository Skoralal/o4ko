using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace o4ko
{
    public class Highlight
    {
        public Rectangle Rectangle { get; private set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public string Name { get; set; }       // Name of the highlight
        public string DataType { get; set; }   // Data type of the highlight

        public string FieldName { get; set; }  // You can assign a name to this highlight field.

        public string RecognizedText { get; set; }

        public Highlight(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Create a new Rectangle for highlighting
            Rectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0)), // Semi-transparent red fill
                Width = width,
                Height = height
            };

            // Set the initial position of the rectangle
            Canvas.SetLeft(Rectangle, X);
            Canvas.SetTop(Rectangle, Y);
            Rectangle.MouseRightButtonDown += OnRightClick;
        }
        public Highlight()
        {

            // Create a new Rectangle for highlighting
            Rectangle = new Rectangle
            {
            };
        }
        private void OnRightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Raise an event or call a callback (to be handled in the main window)
            HighlightRightClicked?.Invoke(this, EventArgs.Empty);
        }
        public void UpdateSize(double width, double height)
        {
            if (width < 0)
            {
                X = X + Width + width;
                Width = width * -1;
                Rectangle.Width = width * -1;
            }
            else
            {
                Width = width;
                Rectangle.Width = width;
            }
            if (height < 0)
            {
                Y = Y + Height + height;
                Height = height * -1;
                Rectangle.Height = height * -1;
            }
            else
            {
                Height = height;
                Rectangle.Height = height;
            }

        }
        public static event EventHandler HighlightRightClicked;
        public void UpdatePosition(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            Canvas.SetLeft(Rectangle, X);
            Canvas.SetTop(Rectangle, Y);
        }
    }
}