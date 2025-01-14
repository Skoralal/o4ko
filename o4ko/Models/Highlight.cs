using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace o4ko
{
    public class Highlight
    {
        public Rectangle Rectangle { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public string Name { get; set; }       // Name of the highlight
        public string DataType { get; set; }   // Data type of the highlight

        public string FieldName { get; set; }  // You can assign a name to this highlight field.

        public string RecognizedText { get; private set; }

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
        }

        public void UpdateSize(double width, double height)
        {
            Width = width;
            Height = height;
            Rectangle.Width = width;
            Rectangle.Height = height;
        }

        public void UpdatePosition(double offsetX, double offsetY)
        {
            X += offsetX;
            Y += offsetY;
            Canvas.SetLeft(Rectangle, X);
            Canvas.SetTop(Rectangle, Y);
        }
    }
}