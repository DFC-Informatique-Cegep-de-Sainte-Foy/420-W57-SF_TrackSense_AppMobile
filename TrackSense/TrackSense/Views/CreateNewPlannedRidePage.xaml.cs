using Mapsui.Projections;
using Mapsui;
using Mapsui.UI.Maui;
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
    IList<Position> Positions { get; }

    IGeolocation _geolocation;
    public Sensor.Location lastLocation;
    public LocationService locationService;


    private MapView mapControl;
    private MemoryLayer markerLayer;
    private Microsoft.Maui.Graphics.Point markerPosition;

    //public Mapsui.UI.Maui.MapControl mapControl;
    //MapControl mapControl = new MapControl();

    public CreateNewPlannedRidePage(CreateNewPlannedRideViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        //locationService = new LocationService();
        //location = locationService.GetLocationAsync().Result;
        /*
        double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        animation = new Animation(v => receptionImg.TranslationX = v,
            -40, screenWidth, Easing.SinIn);

        BindingContext = viewModel;

        viewModel.PropertyChanged += viewModel_PropertyChanged;*/
    }

    public async void DisplayMap(List<Models.PlannedRidePoint> points)
    {
        MapControl mapControl = new Mapsui.UI.Maui.MapControl();
        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        var map = new Mapsui.Map();
        markerLayer = new MemoryLayer();
        map.Layers.Add(markerLayer);

        if (points.Count > 1)
        {
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
        }
        else if(points.Count == 1)
        {
            MPoint point = new MPoint(points.SingleOrDefault().Location.Longitude, points.SingleOrDefault().Location.Latitude);
            mapControl.Map.Home = n => n.CenterOnAndZoomTo(SphericalMercator.FromLonLat(point.X, point.Y).ToMPoint(), 2);
            ILayer iconLayer = CreateSingleIconLayer(points.SingleOrDefault().Location);
            mapControl.Map.Layers.Add(iconLayer);
        }
        else
        {
            throw new ArgumentNullException(nameof(points));
        }
        mapControl.Map.Navigator.RotationLock = true;
        mapContainer.Children.Add(mapControl);
    }

    private ILayer CreateSingleIconLayer(Sensor.Location location)
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

        return new MemoryLayer
        {
            Name = "Icons layer",
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private ILayer CreateSingleStaticIconLayer()
    {
        //IFeature startFeature = new PointFeature(SphericalMercator.FromLonLat(location.Longitude, location.Latitude).ToMPoint());

        int bitMapIdStart = typeof(App).LoadBitmapId("Resources.Images.interest_dark.png");
        var bitmapHeight = 176;
        SymbolStyle symboleStyleStart = new SymbolStyle { BitmapId = bitMapIdStart, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };

        //startFeature.Styles.Add(symboleStyleStart);

        List<IFeature> features = new List<IFeature>()
        {
            //startFeature,
        };

        return new MemoryLayer
        {
            Name = "Icons layer",
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private ILayer CreateIconsLayer(Sensor.Location start, Sensor.Location end)
    {
        IFeature startFeature = new PointFeature(SphericalMercator.FromLonLat(start.Longitude, start.Latitude).ToMPoint());
        IFeature endFeature = new PointFeature(SphericalMercator.FromLonLat(end.Longitude, end.Latitude).ToMPoint());
        int bitMapIdStart = typeof(App).LoadBitmapId("Resources.Images.start_icon.svg");
        int bitMapIdEnd = typeof(App).LoadBitmapId("Resources.Images.end_icon.svg");
        var bitmapHeight = 176;
        SymbolStyle symboleStyleStart = new SymbolStyle { BitmapId = bitMapIdStart, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
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

    private ILayer CreateLineStringLayer(List<Models.PlannedRidePoint> points, IStyle? style = null)
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // SQDC Ste-foy 46.7808056, -71.299583
        //location = new Sensor.Location(46.7808056, -71.299583);

        plannedRidePoints = new();
        locationService = new LocationService();
        lastLocation = await locationService.GetLocationAsync();

        plannedRidePoints.Clear();
        plannedRidePoints.Add(new Models.PlannedRidePoint(1, lastLocation, null, null));
        DisplayMap(plannedRidePoints);

        /*
        if (BindingContext is CreateNewPlannedRideViewModel viewModel)
        {
            viewModel.NewPlannedRide.PlannedRidePoints = plannedRidePoints;

            Models.PlannedRide rideToDisplay = viewModel.NewPlannedRide;
            if (rideToDisplay.PlannedRidePoints.Count > 0)
            {
                DisplayMap(rideToDisplay.PlannedRidePoints);
            }
        }*/
    }

    private async void CreerLeTrajetButton_Clicked(object sender, EventArgs e)
    {
        //await this.BindingContext.CreateNewPlannedRideCommande
    }

    private void PlacerLePointButton_Clicked(object sender, EventArgs e)
    {

    }

    private void NouveauPointButton_Clicked(object sender, EventArgs e)
    {
        if ((sender as Button).Text == "Nouveau point")
        {
            (sender as Button).Text = "I was just clicked!";
        }
        else if ((sender as Button).Text == "I was just clicked!")
        {
            (sender as Button).Text = "Nouveau point";
        }


    }
    private async void AnnulerButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MainPage));
    }
    /*
    private ILayer AddMarker(Microsoft.Maui.Graphics.Point position)
    {
        int bitmapId = typeof(App).LoadBitmapId("Resources.Images.cross_hair.svg");
        IFeature point = new PointFeature(position.X, position.Y);

        SymbolStyle symboleStyle = new SymbolStyle { 
            SymbolType = SymbolType.Image,
            BitmapId = bitmapId,
            SymbolScale = 0.20 
        };

        point.Styles.Add(symboleStyle);

        List<IFeature> features = new List<IFeature>()
        {
            point,
        };

        return new MemoryLayer
        {
            Name = "Crosshair layer",
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };
    }*/
    /*
    private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (BindingContext is CreateNewPlannedRideViewModel viewModel)
        {
            if (e.PropertyName == nameof(viewModel.IsConnected))
            {
                if (viewModel.IsConnected)
                {
                    animation.Commit(this, "animate", 16, 2500, Easing.SinIn,
                        (v, c) => receptionImg.TranslationX = -40, () => true);
                }
                else
                {
                    this.AbortAnimation("animate");
                }
            }
        }
    }*/

}