using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tesseract;

namespace o4ko.Helpers
{
    public static class HighlightReader
    {
        /// <summary>
        /// Reads text from a specific highlight region in the image.
        /// </summary>
        /// <param name="highlight">The highlight to process.</param>
        /// <param name="imageSource">The source image.</param>
        /// <returns>The recognized text from the highlight area.</returns>
        public static string ReadTextFromHighlight(Highlight highlight, BitmapSource imageSource)
        {
            if (highlight == null || imageSource == null)
                throw new ArgumentNullException("Highlight or image source cannot be null.");

            try
            {
                // Crop the image to the highlight's region
                var croppedBitmap = ResizeImage( CropImage(imageSource, highlight),8);
                    using (var fileStream = new FileStream("aboba.png", FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder(); // You can use JpegBitmapEncoder, etc.
                        encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                        encoder.Save(fileStream);
                    }
                // Convert cropped bitmap to a format Tesseract understands
                var bitmapForOcr = BitmapSourceToByteArray(croppedBitmap);

                // Perform OCR on the cropped image
                var aboba = PerformOcr(bitmapForOcr);
                highlight.RecognizedText = aboba;
                return aboba;
            }
            catch (Exception ex)
            {
                return $"Error reading text: {ex.Message}";
            }
        }

        private static BitmapSource CropImage(BitmapSource source, Highlight highlight)
        {
            // Calculate cropping rectangle
            var rect = new Int32Rect(
                (int)highlight.X,
                (int)highlight.Y,
                (int)highlight.Width,
                (int)highlight.Height
            );

            // Crop the image
            var croppedBitmap = new CroppedBitmap(source, rect);
            return croppedBitmap;
        }

        private static byte[] BitmapSourceToByteArray(BitmapSource bitmapSource)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string PerformOcr(byte[] imageBytes)
        {
            // Path to Tesseract data folder (make sure it's configured correctly)
            string tesseractDataPath = @"Tess";

            using (var engine = new TesseractEngine(tesseractDataPath, "eng", EngineMode.Default))
            {
                using (var image = Pix.LoadFromMemory(imageBytes))
                using (var page = engine.Process(image))
                {
                    string aboba = page.GetText();
                    return aboba.Trim();
                }
            }
        }
        public static BitmapSource ResizeImage(BitmapSource originalImage, int scaleFactor)
        {
            // Create a scale transform
            var scaleTransform = new ScaleTransform(4, 4);

            // Apply the transform using TransformedBitmap
            var stretchedBitmap = new TransformedBitmap(originalImage, scaleTransform);
            return stretchedBitmap;
            
        }
    }
}