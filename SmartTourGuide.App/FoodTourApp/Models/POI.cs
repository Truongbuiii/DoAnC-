using FoodTourApp.Services;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodTourApp.Models
{
    public class POI : INotifyPropertyChanged
    {
        [PrimaryKey]
        public int PoiId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TriggerRadius { get; set; }
        public string ImageSource { get; set; } = string.Empty;

        // CHỈ GIỮ TIẾNG VIỆT
        public string DescriptionVi { get; set; } = string.Empty;

        private string? _displayName;
        [Ignore]
        public string? DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        private string? _displayCategory;
        [Ignore]
        public string? DisplayCategory
        {
            get => _displayCategory;
            set => SetProperty(ref _displayCategory, value);
        }

        [Ignore]
        public string FullImageUrl => string.IsNullOrEmpty(ImageSource)
            ? ""
            : ImageSource.StartsWith("http")
                ? ImageSource
                : $"{ApiSyncService.BaseUrl}/images/{ImageSource}";

        private string _distanceDisplay = string.Empty;
        [Ignore]
        public string DistanceDisplay
        {
            get => _distanceDisplay;
            set => SetProperty(ref _distanceDisplay, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
