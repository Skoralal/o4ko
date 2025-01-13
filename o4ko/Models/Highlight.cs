using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace o4ko
{
    public class Highlight
    {
        public Rectangle Rectangle { get; private set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public Highlight(double startX, double startY, double width, double height)
        {
            StartX = startX;
            StartY = startY;
            Width = width;
            Height = height;

            // Initialize the visual representation of the highlight (as a rectangle)
            Rectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)) // Semi-transparent red
            };
            Canvas.SetLeft(Rectangle, StartX);
            Canvas.SetTop(Rectangle, StartY);
            Rectangle.Width = Width;
   