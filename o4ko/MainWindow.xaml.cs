using o4ko.Helpers;
using o4ko.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace o4ko
{
    public partial class MainWindow : Window
    {
        private ImageModel _imageModel;
        private bool _isDragging = false;
        private Point _clickPosition;
        private Image _currentImage;
        public MainWindow()
        {
            InitializeComponent();
            _imageModel = new ImageModel();
            _currentImage = new Image();
            _currentImage.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            _currentImage.MouseMove += Image_MouseMove;
            _currentImage.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            _currentImage.MouseWheel += Image_MouseWheel;
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            string imagePath = FileManager.OpenImageDialog();

            if (!string.IsNullOrEmpty(imagePath))
            {
                _imageModel.ImagePath = imagePath;
                LoadImage();
            }
        }

        private void LoadImage()
        {
            if (FileManager.FileExists(_imageModel.ImagePath))
            {
                BitmapImage bitmap = new BitmapImage(new Uri(_imageModel.ImagePath));
                _currentImage.Source = bitmap;
                _currentImage.Width = bitmap.PixelWidth / 2; // Adjust size if needed
                _currentImage.Height = bitmap.PixelHeight / 2;

                // Center the image in the canvas
                double canvasCenterX = WorkingCanvas.Width / 2;
                double canvasCenterY = WorkingCanvas.Height / 2;
                Canvas.SetLeft(_currentImage, canvasCenterX - _currentImage.Width / 2);
                Canvas.SetTop(_currentImage, canvasCenterY - _currentImage.Height / 2);

                // Clear existing children and add the image
                WorkingCanvas.Children.Clear();
                WorkingCanvas.Children.Add(_currentImage);
            }
            else
            {
                MessageBox.Show("File does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Mouse control for dragging the image
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentImage.IsMouseOver)
            {
                _isDragging = true;
                _clickPosition = e.GetPosition(WorkingCanvas);
                _currentImage.CaptureMouse();
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(WorkingCanvas);
                double offsetX = currentPosition.X - _clickPosition.X;
                double offsetY = currentPosition.Y - _clickPosition.Y;

                double newLeft = Canvas.GetLeft(_currentImage) + offsetX;
                double newTop = Canvas.GetTop(_currentImage) + offsetY;

                Canvas.SetLeft(_currentImage, newLeft);
                Canvas.SetTop(_currentImage, newTop);

                _clickPosition = currentPosition;
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _currentImage.ReleaseMouseCapture();
            }
        }

        // Zooming the image using Ctrl + Mouse Wheel
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Check if the Ctrl key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9; // Zoom in (1.1x) or zoom out (0.9x)

                // Apply the zoom factor to image width and height
                _currentImage.Width *= zoomFactor;
                _currentImage.Height *= zoomFactor;

                // Optionally, adjust the position to keep the image centered (if desired)
                double canvasCenterX = WorkingCanvas.Width / 2;
                double canvasCenterY = WorkingCanvas.Height / 2;
                Canvas.SetLeft(_currentImage, canvasCenterX - _currentImage.Width / 2);
                Canvas.SetTop(_currentImage, canvasCenterY - _currentImage.Height / 2);
            }
        }
    }
}