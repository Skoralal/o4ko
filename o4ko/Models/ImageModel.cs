using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace o4ko.Models
{
    public class ImageModel : INotifyPropertyChanged
    {
        private string _imagePath;
        private double _positionX;
        private double _positionY;
        public List<Highlight> Highlights { get; set; } = new List<Highlight>();
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public double PositionX
        {
            get => _positionX;
            set
            {
                _positionX = value;
                OnPropertyChanged();
            }
        }

        public double PositionY
        {
            get => _positionY;
            set
            {
                _positionY = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}