using Mapsui.Projections;
using Mapsui;
using Mapsui.UI.Maui;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Diagnostics;
using System.ComponentModel;
using TrackSense.ViewModels;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Color = Mapsui.Styles.Color;
using Mapsui.Nts.Extensions;
using Sensor = Microsoft.Maui.Devices.Sensors;
using Position = Mapsui.UI.Maui.Position;
using TrackSense.Services;
using Mapsui.Providers;
using Microsoft.Maui.ApplicationModel;

namespace TrackSense.Views;

public partial class CreateNewPlannedRidePage : ContentPage
{
    readonly Animation animation;
    List<Models.PlannedRidePoint> plannedRidePoints;

    public Sensor.Location lastLocation = new Sensor.Location();
    public Sensor.Location currentLocation = new Sensor.Location();
    public LocationService locationService;
    private MapControl mapControl;

    public CreateNewPlannedRidePage(CreateNewPlannedRideViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        plannedRidePoints = new List<Models.PlannedRidePoint>();
        InitializeMap();
    }

    private void InitializeMap()
    {
        mapControl = new MapControl
        {
            Map = new Mapsui.Map()
        };
        mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        mapControl.Info += MapControl_Info;
        mapContainer.Children.Add(mapControl);

        plannedRidePoints.Clear();
        locationService = new LocationService();
    }

    private void MapControl_Info(object sender, MapInfoEventArgs e)
    {
        if (e.MapInfo?.WorldPosition != null)
        {
            var clickedPosition = e.MapInfo.WorldPosition;
            var lonLat = SphericalMercator.ToLonLat(clickedPosition.X, clickedPosition.Y);

            plannedRidePoints.Add(new Models.PlannedRidePoint
            {
                RideStep = this.plannedRidePoints.Count + 1,
                Location = new Sensor.Location(lonLat.ToMPoint().Y, lonLat.ToMPoint().X)
            });

            AddMarkerToMap(clickedPosition);

            if (plannedRidePoints.Count > 1)
            {
                CreateLineStringLayer(plannedRidePoints);
            }

            // DEBUG
            var names = mapControl.Map.Layers.Select(l => l.Name.ToString() + " id#" + l.Id.ToString()).Where(n => n.Length >= 1).ToList();
            var allNames = String.Join(", ", names);
            var idCount = mapControl.Map.Layers.Select(l => l.Id.ToString()).Count();
            Debug.WriteLine($"{idCount} Layers, Names: {allNames}");
            // DEBUG
        }
    }

    private void AddMarkerToMap(MPoint position)
    {
        ILayer markerLayer = new MemoryLayer
        {
            Features = new List<IFeature>
            {
                new PointFeature(position)
                {
                    Styles = new[] { new SymbolStyle { SymbolScale = 0.3 } }
                }
            }
        };
        
        if (plannedRidePoints.Count == 1)
        {
            mapControl.Map.Home = n => n.CenterOnAndZoomTo(SphericalMercator.FromLonLat(position.X, position.Y).ToMPoint(), 2);
            CreateSingleIconLayer(plannedRidePoints.First().Location);
        }
        else
        {
            mapControl.Map.Layers.Add(markerLayer);
        }
        mapControl.Refresh();
    }

    private void CreateSingleIconLayer(Sensor.Location location)
    {
        IFeature startFeature = new PointFeature(SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint());
        int bitMapIdStart = typeof(App).LoadBitmapId("Resources.Images.start_icon.svg");
        var bitmapHeight = 176;
        SymbolStyle symboleStyleStart = new SymbolStyle { BitmapId = bitMapIdStart, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
        startFeature.Styles.Add(symboleStyleStart);

        List<IFeature> features = new List<IFeature>()
        {
            startFeature,
        };

        ILayer IconLayer = new MemoryLayer
        {
            Name = "Icons layer",
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };

        mapControl.Map.Layers.Add(IconLayer);
    }

    private void CreateLineStringLayer(List<Models.PlannedRidePoint> points, IStyle? style = null)
    {
        LineString lineString = new LineString(points.Select(p => SphericalMercator.FromLonLat(p.Location.Longitude, p.Location.Latitude).ToCoordinate()).ToArray());

        if(style == null) 
        { 
            style = new VectorStyle
            {
                Fill = null,
                Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                Line = { Color = Color.FromString("Blue"), Width = 4 }
            };
        }

        ILayer polylineLayer = new MemoryLayer
        {
            Features = new[] { new GeometryFeature { Geometry = lineString } },
            Name = "LineStringLayer",
            Style = style

        };

        mapControl.Map.Layers.Add(polylineLayer);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // SQDC Ste-foy 46.7808056, -71.299583
        //location = new Sensor.Location(46.7808056, -71.299583);

        currentLocation = await locationService.GetLocationAsync();
        plannedRidePoints.Add(new Models.PlannedRidePoint(1, currentLocation, null, null));

        MPoint point = new MPoint(plannedRidePoints.SingleOrDefault().Location.Longitude, plannedRidePoints.SingleOrDefault().Location.Latitude);
        AddMarkerToMap(point);
        mapControl.Map.Navigator.RotationLock = true;


        MainThread.BeginInvokeOnMainThread(() =>
        {
            mapContainer.Children.Clear();
            mapContainer.Children.Add(mapControl);
        });
    }

    private async void CreerLeTrajetButton_Clicked(object sender, EventArgs e)
    {
        //await this.BindingContext.CreateNewPlannedRideCommande
    }

    private async void AnnulerButton_Clicked(object sender, EventArgs e)
    {
        mapControl.Map.Layers.Clear();
        mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        plannedRidePoints.Clear();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    private void DeleteLastMarker_Clicked(object? sender, EventArgs e)
    {
        if (plannedRidePoints.Count > 1)
        {
            plannedRidePoints.RemoveAt(plannedRidePoints.Count - 1);

            mapControl.Map.Layers.Remove(mapControl.Map.Layers.Last());
            mapControl.Map.Layers.Remove(mapControl.Map.Layers.Last());
        }
        else if (plannedRidePoints.Count == 1)
        {
            plannedRidePoints.Clear();
            mapControl.Map.Layers.Remove(mapControl.Map.Layers.Last());
        }

        var names = mapControl.Map.Layers.Select(l => l.Name.ToString() + " id#" + l.Id.ToString()).Where(n => n.Length >= 1).ToList();
        var allNames = String.Join(", ", names);
        var idCount = mapControl.Map.Layers.Select(l => l.Id.ToString()).Count();
        Debug.WriteLine($"{idCount} Layers, Names: {allNames}");
        Debug.WriteLine($"Positions: {plannedRidePoints.Count}");
    }

}