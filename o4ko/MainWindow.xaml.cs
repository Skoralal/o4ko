using o4ko.Helpers;
using o4ko.Models;
using o4ko.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace o4ko
{
    public partial class MainWindow : Window
    {
        private bool _isDragging = false;
        private Point _clickPosition;
        private Image _currentImage;
        private List<Highlight> _highlights = new List<Highlight>();
        private Highlight _currentHighlight;
        private Point _rectStartPoint;
        private bool _isDrawing = false;
        private double _imageScaleX = 1;
        private double _imageScaleY = 1;
        private Point _lastMousePosition;
        private bool _isPanning;
        // Declare ImageModel
        private ImageModel _imageModel;
        private Point _startPoint;
        public MainWindow()
        {
            InitializeComponent();

            // Initialize the ImageModel
            _imageModel = new ImageModel();

            _currentImage = new Image();
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            string imagePath = FileManager.OpenImageDialog();

            if (!string.IsNullOrEmpty(imagePath))
            {
                _imageModel.ImagePath = imagePath;  // Set the image path in the model
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

                // Update the scale factors based on the image size
                _imageScaleX = _currentImage.Width / bitmap.PixelWidth;
                _imageScaleY = _currentImage.Height / bitmap.PixelHeight;

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
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Get the current mouse position relative to the canvas
            Point mousePosition = e.GetPosition(WorkingCanvas);

            // Adjust the zoom factor
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            // Apply the scale transformation
            CanvasScaleTransform.ScaleX *= zoomFactor;
            CanvasScaleTransform.ScaleY *= zoomFactor;

            // Adjust the canvas position to zoom in/out at the mouse pointer
            CanvasTranslateTransform.X = (CanvasTranslateTransform.X - mousePosition.X) * zoomFactor + mousePosition.X;
            CanvasTranslateTransform.Y = (CanvasTranslateTransform.Y - mousePosition.Y) * zoomFactor + mousePosition.Y;
        }
        // Mouse control for dragging the image
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if Shift key is pressed to enable drawing highlights
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                _isDrawing = true;

                // Convert mouse position to canvas coordinates considering transformations
                _startPoint = e.GetPosition(WorkingCanvas);
                Point canvasPoint = _startPoint;

                // Create a new highlight
                _currentHighlight = new Highlight(canvasPoint.X, canvasPoint.Y, 0, 0);
                WorkingCanvas.Children.Add(_currentHighlight.Rectangle);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastMousePosition = e.GetPosition(this);
                WorkingCanvas.CaptureMouse();
            }
        }

        // Handle Mouse Move for Panning
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(WorkingCanvas);
            MousePositionTextBlock.Text = $"X: {mousePosition.X:F2}, Y: {mousePosition.Y:F2}";
            if (_isPanning && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentMousePosition = e.GetPosition(this);

                // Calculate the offset
                double offsetX = currentMousePosition.X - _lastMousePosition.X;
                double offsetY = currentMousePosition.Y - _lastMousePosition.Y;

                // Apply the translation transformation
                CanvasTranslateTransform.X += offsetX;
                CanvasTranslateTransform.Y += offsetY;

                // Update the last mouse position
                _lastMousePosition = currentMousePosition;
            }
            if (_isDrawing && _currentHighlight != null)
            {
                // Convert mouse position to canvas coordinates
                Point currentPosition = e.GetPosition(WorkingCanvas);
                Point canvasPoint = currentPosition;

                // Calculate width and height based on mouse movement
                double width = Math.Abs(canvasPoint.X - _currentHighlight.X);
                double height = Math.Abs(canvasPoint.Y - _currentHighlight.Y);

                // Update highlight dimensions
                _currentHighlight.UpdateSize(width, height);

                // Adjust position for dragging in reverse direction
                if (canvasPoint.X < _startPoint.X)
                {
                    Canvas.SetLeft(_currentHighlight.Rectangle, canvasPoint.X);
                }

                if (canvasPoint.Y < _startPoint.Y)
                {
                    Canvas.SetTop(_currentHighlight.Rectangle, canvasPoint.Y);
                }
            }
        }

        // Handle Mouse Left Button Up to Stop Panning
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;

                if (_currentHighlight != null)
                {
                    // Show dialog to get highlight properties
                    var dialog = new HighlightPropertiesDialog
                    {
                        Owner = this
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        // Set name and datatype for the highlight
                        _currentHighlight.Name = dialog.HighlightName;
                        _currentHighlight.DataType = dialog.DataType;

                        // Add to the list
                        _highlights.Add(_currentHighlight);
                    }
                    else
                    {
                        // If the dialog is canceled, remove the highlight
                        WorkingCanvas.Children.Remove(_currentHighlight.Rectangle);
                    }

                    _currentHighlight = null;
                }
            }
            if (_isPanning)
            {
                _isPanning = false;
                WorkingCanvas.ReleaseMouseCapture();
            }
        }
    }
}