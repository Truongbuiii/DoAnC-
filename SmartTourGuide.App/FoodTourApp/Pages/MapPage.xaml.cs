using FoodTourApp.Models;
using FoodTourApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace FoodTourApp.Pages;

public partial class MapPage : ContentPage
{
    POIService poiService = new POIService();
    bool isTracking = true;
    public MapPage()
    {
        InitializeComponent();

        LoadPOIs();
        
        StartTracking();

    }
    void LoadPOIs()
    {
        var pois = poiService.GetPOIs();

        foreach (var poi in pois)
        {
            var pin = new Pin
            {
                Label = poi.Name,
                Address = poi.Description,
                Location = new Location(poi.Latitude, poi.Longitude),
                Type = PinType.Place
            };

            FoodMap.Pins.Add(pin);
        }
    }

    async Task StartTracking()
    {
        while (isTracking)
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(5));

            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                UpdateUserLocation(location);
            }

            await Task.Delay(3000);
        }
    }


    void UpdateUserLocation(Location location)
    {
        var mapLocation = new Location(location.Latitude, location.Longitude);

        var span = new MapSpan(mapLocation, 0.01, 0.01);

        FoodMap.MoveToRegion(span);

        // tìm POI gần nhất
        var nearest = FindNearestPOI(location);

        if (nearest != null)
        {
            DisplayAlert("POI gần nhất", nearest.Name, "OK");
        }
    }

    POI FindNearestPOI(Location userLocation)
    {
        var pois = poiService.GetPOIs();

        POI nearest = null;
        double minDistance = double.MaxValue;

        foreach (var poi in pois)
        {
            var poiLocation = new Location(poi.Latitude, poi.Longitude);

            double distance = Location.CalculateDistance(
                userLocation,
                poiLocation,
                DistanceUnits.Kilometers);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = poi;
            }
        }

        return nearest;
    }
}