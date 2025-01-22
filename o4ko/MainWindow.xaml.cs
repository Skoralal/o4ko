using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using o4ko.Helpers;
using o4ko.Models;
using o4ko.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace o4ko
{
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        private double _startX;
        private double _startY;
        private BatchReader _batchReader;
        private string _loadedPresetName = "Nameless";
        private List<ImageModel> _imageModels = new List<ImageModel>();
        private int _currentImageIndex = 0; // Index of the currently displayed image
        List<object> _dynamicInstances = new List<object>();
        string ImageCounterTextBlock { get => $"{_currentImageIndex + 1}/{_imageModels.Count}"; }
        public int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                if (_currentImageIndex != value)
                {
                    _currentImageIndex = value;
                    OnPropertyChanged(nameof(CurrentImageIndex));
                    OnPropertyChanged(nameof(ImageCounterText)); // Update combined property
                }
            }
        }

        private int _totalImageCount;
        public int TotalImageCount
        {
            get => _totalImageCount;
            set
            {
                if (_totalImageCount != value)
                {
                    _totalImageCount = value;
                    OnPropertyChanged(nameof(TotalImageCount));
                    OnPropertyChanged(nameof(ImageCounterText)); // Update combined property
                }
            }
        }

        public string ImageCounterText => $"{CurrentImageIndex + 1}/{TotalImageCount}";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            // Initialize the ImageModel
            _imageModel = new ImageModel();
            Highlight.HighlightRightClicked += Highlight_RightClicked;
            _currentImage = new Image();
            _batchReader = new BatchReader();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            var imagePaths = FileManager.OpenMultipleImageDialog();

            if (imagePaths != null && imagePaths.Any())
            {
                foreach (var imagePath in imagePaths)
                {
                    _imageModels.Add(new ImageModel { ImagePath = imagePath });
                }

                // Load the first image
                _currentImageIndex = 0;
                LoadImage(_currentImageIndex);
            }
        }

        private void LoadImage(int index)
        {
            if (index >= 0 && index < _imageModels.Count)
            {
                var imageModel = _imageModels[index];

                if (FileManager.FileExists(imageModel.ImagePath))
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(imageModel.ImagePath));
                    _currentImage.Source = bitmap;
                    _currentImage.Width = bitmap.PixelWidth; // Adjust size if needed
                    _currentImage.Height = bitmap.PixelHeight;

                    // Center the image in the canvas
                    double canvasCenterX = WorkingCanvas.Width;
                    double canvasCenterY = WorkingCanvas.Height;
                    Canvas.SetLeft(_currentImage, canvasCenterX - _currentImage.Width);
                    Canvas.SetTop(_currentImage, canvasCenterY - _currentImage.Height);

                    // Clear and add the image to the canvas
                    WorkingCanvas.Children.Clear();
                    WorkingCanvas.Children.Add(_currentImage);
                    if (_highlights.Count == 0)
                    {
                        foreach (var highlight in imageModel.Highlights)
                        {

                            //_highlights.Add(highlight);
                            WorkingCanvas.Children.Add(highlight.Rectangle);
                            HighlightReader.ReadTextFromHighlight(highlight, (BitmapSource)_currentImage.Source);
                            UpdateHighlightsDisplay();
                        }
                    }
                    TotalImageCount = _imageModels.Count;
                }
                else
                {
                    MessageBox.Show($"File does not exist: {imageModel.ImagePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
                _startX = _startPoint.X;
                _startY = _startPoint.Y;
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
                double width = canvasPoint.X - _startX;
                //if(canvasPoint.X - _currentHighlight.X < 0)
                //{
                //    _currentHighlight.X = canvasPoint.X;
                //    width += _currentImage.Width / 190;
                //}
                double height = canvasPoint.Y - _startY;
                //if (canvasPoint.Y - _currentHighlight.Y < 0)
                //{
                //    _currentHighlight.Y = canvasPoint.Y;
                //    height += _currentImage.Height / 100;
                //}
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
                    HighlightReader.ReadTextFromHighlight(_currentHighlight, (BitmapSource)_currentImage.Source);
                    UpdateHighlightsDisplay();

                    _currentHighlight = null;
                }
            }
            if (_isPanning)
            {
                _isPanning = false;
                WorkingCanvas.ReleaseMouseCapture();
            }
        }
        private void Highlight_RightClicked(object sender, EventArgs e)
        {
            if (sender is Highlight highlight)
            {
                // Remove the highlight from the canvas
                WorkingCanvas.Children.Remove(highlight.Rectangle);

                // Remove the highlight from the list
                _highlights.Remove(highlight);
                UpdateHighlightsDisplay();
            }
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            // Prompt the user for a preset name
            string presetName = PromptUserForPresetName();

            if (!string.IsNullOrEmpty(presetName))
            {
                try
                {
                    SavePreset(presetName);
                    MessageBox.Show($"Preset '{presetName}' saved successfully.",
                                    "Save Preset",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save preset: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Preset name cannot be empty.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void SavePreset(string presetName)
        {
            // Define the folder and file path
            string presetsFolder = "Presets";
            if (!System.IO.Directory.Exists(presetsFolder))
            {
                System.IO.Directory.CreateDirectory(presetsFolder);
            }

            string filePath = System.IO.Path.Combine(presetsFolder, $"{presetName}.json");

            // Serialize the highlights
            var highlightsData = _highlights.Select(h => new HighlightData
            {
                X = Canvas.GetLeft(h.Rectangle),
                Y = Canvas.GetTop(h.Rectangle),
                Width = h.Rectangle.Width,
                Height = h.Rectangle.Height,
                Name = h.Name,
                DataType = h.DataType
            });

            string jsonData = System.Text.Json.JsonSerializer.Serialize(highlightsData,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            // Save the JSON data to a file
            System.IO.File.WriteAllText(filePath, jsonData);
        }

        private string PromptUserForPresetName()
        {
            // Use a simple input dialog to get the name
            InputDialog inputDialog = new InputDialog("Enter Preset Name:", "Save Preset");
            if (inputDialog.ShowDialog() == true)
            {
                return inputDialog.ResponseText;
            }
            return null;
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select a preset
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets"),
                Title = "Select a Preset File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    LoadPreset(filePath);
                    _loadedPresetName = filePath.Split('\\')[^1].Split('.')[0];
                    MessageBox.Show($"Preset loaded successfully from '{filePath}'.",
                                    "Load Preset",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load preset: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
        private void LoadPreset(string filePath)
        {
            _highlights.Clear();
            for (int i = WorkingCanvas.Children.Count-1; i > 0; i--)
            {
                WorkingCanvas.Children.RemoveAt(i);
            }
            foreach (var image in _imageModels)
            {
                image.Highlights.Clear();
            }
            // Read and deserialize the JSON data
            string jsonData = System.IO.File.ReadAllText(filePath);
            var loadedHighlights = System.Text.Json.JsonSerializer.Deserialize<List<HighlightData>>(jsonData);

            if (loadedHighlights != null)
            {
                // Clear existing highlights and add the loaded ones
                foreach (var highlightData in loadedHighlights)
                {
                    Highlight highlight = new Highlight(highlightData.X, highlightData.Y, highlightData.Width, highlightData.Height)
                    {
                        Name = highlightData.Name,
                        DataType = highlightData.DataType
                    };
                    _highlights.Add(highlight);
                    WorkingCanvas.Children.Add(highlight.Rectangle);
                    HighlightReader.ReadTextFromHighlight(highlight, (BitmapSource)_currentImage.Source);
                    UpdateHighlightsDisplay();
                }
            }
        }
        //
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_highlights.Count == 0)
            {
                MessageBox.Show("No highlights available to generate a class.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _dynamicInstances.Clear();

                var batchReader = new BatchReader();
                var dynamicClass = batchReader.GenerateClass(_highlights, _loadedPresetName);
            //foreach in _imageModels
            foreach (var image in _imageModels)
            {
                var bitmapImage = new BitmapImage(new Uri(image.ImagePath));
                foreach (var highlight in _highlights)
                {
                    var hl = new Highlight(highlight.X, highlight.Y, highlight.Width, highlight.Height)
                    {
                        DataType = highlight.DataType,
                        Name = highlight.Name
                    };
                    HighlightReader.ReadTextFromHighlight(hl, bitmapImage);
                    image.Highlights.Add(hl);
                }
            }

            foreach (var image in _imageModels)
            {
                
                object dynamicObject = Activator.CreateInstance(dynamicClass, _highlights.Select(h => ConvertValue(h.RecognizedText, h.DataType)).ToArray());
                Type dynamicType = dynamicObject.GetType();
                foreach (var highlight in image.Highlights)
                {
                    string propertyName = highlight.Name;
                    if (dynamicType.GetProperty(propertyName) != null)
                    {
                        // Set the property value
                        dynamicType.GetProperty(propertyName).SetValue(dynamicObject, ConvertValue(highlight.RecognizedText, highlight.DataType)); // Replace "New Value" with the actual value
                    }
                }
                _dynamicInstances.Add(dynamicObject);
            }
                
            //end foreach
            // Create an instance of the class with the recognized values


            MessageBox.Show($"Class 'aboba' generated successfully and populated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }

        private object ConvertValue(string value, string dataType)
        {
            return dataType.ToLower() switch
            {
                "int" => int.TryParse(value, out int i) ? i : 0,
                "double" => double.TryParse(value, out double d) ? d : 0,
                "bool" => bool.TryParse(value, out bool b) ? b : false,
                "float" => float.TryParse(value, out float f) ? f : 0f,
                _ => value // Default to string
            };
        }
        private void UpdateHighlightsDisplay()
        {
            // Clear existing children
            HighlightsPanel.Children.Clear();

            // Add each highlight as a horizontal layout
            foreach (var highlight in _highlights)
            {
                StackPanel highlightEntry = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                // Add TextBlock for the name
                TextBlock nameTextBlock = new TextBlock
                {
                    Text = highlight.Name,
                    FontWeight = FontWeights.Bold,
                    Width = 100
                };

                // Add TextBlock for the recognized value
                TextBlock valueTextBlock = new TextBlock
                {
                    Text = highlight.RecognizedText ?? "Not Recognized",
                    Width = 150
                };

                // Add elements to the horizontal layout
                highlightEntry.Children.Add(nameTextBlock);
                highlightEntry.Children.Add(valueTextBlock);

                // Add the entry to the HighlightsPanel
                HighlightsPanel.Children.Add(highlightEntry);
            }
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(_dynamicInstances, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            ExportWindow exportWindow = new ExportWindow(json, "JSON");
            exportWindow.ShowDialog();
        }

        private void ExportXml_Click(object sender, RoutedEventArgs e)
        {
            string ConvertJsonToXml(string jsonArrayString)
            {
                // Parse the JSON array
                JArray jsonArray = JArray.Parse(jsonArrayString);

                // Wrap the array in a root element for valid XML structure
                var wrappedObject = new JObject { ["Items"] = jsonArray };

                // Convert to XML
                XmlDocument xmlDocument = JsonConvert.DeserializeXmlNode(wrappedObject.ToString(), "Root");

                // Return formatted XML
                return xmlDocument.OuterXml;
            }
            using (StringWriter textWriter = new StringWriter())
            {
                string xml = ConvertJsonToXml(System.Text.Json.JsonSerializer.Serialize(_dynamicInstances, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));

                ExportWindow exportWindow = new ExportWindow(xml, "XML");
                exportWindow.ShowDialog();
            }
        }

        private void ExportSql_Click(object sender, RoutedEventArgs e)
        {
            string sql = GenerateSqlInsertQuery();

            ExportWindow exportWindow = new ExportWindow(sql, "TXT");
            exportWindow.ShowDialog();
        }

        private string GenerateSqlInsertQuery()
        {
            if (_dynamicInstances == null || _dynamicInstances.Count == 0)
                throw new ArgumentException("Invalid data for SQL generation.");
            var fieldNames = _dynamicInstances[0].GetType().GetProperties().Select(p => p.Name).ToList();
            // Start building the query
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"INSERT INTO {_loadedPresetName} ({string.Join(", ", fieldNames)}) VALUES");

            // Iterate through instances and add their values
            for (int i = 0; i < _dynamicInstances.Count; i++)
            {
                var instance = _dynamicInstances[i];
                List<string> values = new List<string>();

                foreach (var fieldName in fieldNames)
                {
                    var value = instance.GetType().GetProperty(fieldName)?.GetValue(instance);
                    if (value is string)
                    {
                        values.Add($"'{value.ToString().Replace("'", "''")}'"); // Escape single quotes
                    }
                    else if (value == null)
                    {
                        values.Add("NULL");
                    }
                    else
                    {
                        values.Add(value.ToString());
                    }
                }

                // Format values and append
                queryBuilder.Append($"({string.Join(", ", values)})");

                if (i < _dynamicInstances.Count - 1)
                    queryBuilder.AppendLine(","); // Add a comma between rows
                else
                    queryBuilder.AppendLine(";"); // End the query with a semicolon
            }

            return queryBuilder.ToString();
        }

        private void PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            if(_currentImageIndex > 0)
            {
                _currentImageIndex--;
                OnPropertyChanged(nameof(ImageCounterText));
                LoadImage(_currentImageIndex);
                foreach (var highlight in _highlights)
                {
                    if (!WorkingCanvas.Children.Contains(highlight.Rectangle))
                    {
                        WorkingCanvas.Children.Add(highlight.Rectangle);
                    }
                    HighlightReader.ReadTextFromHighlight(highlight, (BitmapSource)_currentImage.Source);
                    UpdateHighlightsDisplay();
                }
            }
        }

        private void NextImage_Click(object sender, RoutedEventArgs e)
        {
            if( _currentImageIndex < _imageModels.Count -1 )
            {
                _currentImageIndex++;
                OnPropertyChanged(nameof(ImageCounterText));
                LoadImage(_currentImageIndex);
                foreach (var highlight in _highlights)
                {
                    if (!WorkingCanvas.Children.Contains(highlight.Rectangle))
                    {
                        WorkingCanvas.Children.Add(highlight.Rectangle);
                    }
                    HighlightReader.ReadTextFromHighlight(highlight, (BitmapSource)_currentImage.Source);
                    UpdateHighlightsDisplay();
                }
            }
        }
    }
}