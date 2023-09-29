using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using NetTopologySuite.Geometries;
using TrackSense.Models;
using TrackSense.ViewModels;
using Color = Mapsui.Styles.Color;

namespace TrackSense.Views;

public partial class CompletedRidePage : ContentPage
{
	public CompletedRidePage(CompletedRideViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

	public void DisplayMap(List<CompletedRidePoint> points)
	{
        MapControl mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        ILayer lineStringLayer = CreateLineStringLayer(points, CreateLineStringStyle());
        mapControl.Map.Layers.Add(lineStringLayer);

        ILayer iconsLayer = CreateIconsLayer(points.First().Location, points.Last().Location);
        mapControl.Map.Layers.Add(iconsLayer);

        double boxSize = lineStringLayer.Extent!.Width > lineStringLayer.Extent.Height ? lineStringLayer.Extent.Width : lineStringLayer.Extent.Height;
        double resolution = boxSize / 256;
        if (resolution < 1)
        {
            resolution = 1;
        }
        mapControl.Map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, resolution);

        mapControl.Map.Navigator.RotationLock = true;
        mapContainer.Children.Add(mapControl);
    }

    private ILayer CreateIconsLayer(Microsoft.Maui.Devices.Sensors.Location start, Microsoft.Maui.Devices.Sensors.Location end)
    {
        IFeature startFeature = new PointFeature(SphericalMercator.FromLonLat(start.Longitude, start.Latitude).ToMPoint());
        IFeature endFeature = new PointFeature(SphericalMercator.FromLonLat(end.Longitude, end.Latitude).ToMPoint());
        int bitMapIdStart = typeof(App).LoadBitmapId("Resources.Images.start_icon.svg");
        int bitMapIdEnd = typeof(App).LoadBitmapId("Resources.Images.end_icon.svg");
        var bitmapHeight = 176;
        SymbolStyle symboleStyleStart = new SymbolStyle { BitmapId = bitMapIdStart, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5)};
        SymbolStyle symboleStyleEnd = new SymbolStyle { BitmapId = bitMapIdEnd, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
        startFeature.Styles.Add(symboleStyleStart);
        endFeature.Styles.Add(symboleStyleEnd);

        List<IFeature> features = new List<IFeature>()
        {
            startFeature,
            endFeature
        };

        return new MemoryLayer
        {
            Name = "Icons layer",
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private ILayer CreateLineStringLayer(List<CompletedRidePoint> points, IStyle? style = null)
    {
        LineString lineString = new LineString(points.Select(p => SphericalMercator.FromLonLat(p.Location.Longitude, p.Location.Latitude).ToCoordinate()).ToArray());

        return new MemoryLayer
        {
            Features = new[] { new GeometryFeature { Geometry = lineString } },
            Name = "LineStringLayer",
            Style = style

        };
    }

    private IStyle CreateLineStringStyle()
    {
        return new VectorStyle
        {
            Fill = null,
            Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
            Line = { Color = Color.FromString("Red"), Width = 4 }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CompletedRideViewModel viewModel)
        {
            CompletedRide rideToDisplay = viewModel.CompletedRide;
            DisplayMap(rideToDisplay.CompletedRidePoints);
        }
    }
}