using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using System.Threading;
using Xamarin.Forms;

namespace QueryBug
{
    public partial class MainPage : ContentPage
    {
        private GraphicsOverlay _overlay = new GraphicsOverlay();

        public MainPage()
        {
            InitializeComponent();

            myMapView.Map = new Map(new Uri("https://latitudegeo.maps.arcgis.com/home/item.html?id=0ba877a4185448cb832af9a661031e31"));
            myMapView.GraphicsOverlays.Add(_overlay);
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            var geometry = GeometryEngine.Buffer(e.Location, 150);
            var hydrantsLayer = myMapView.Map.AllLayers.FirstOrDefault(layer => layer.Name == "Fire Hydrants") as FeatureLayer;

            // Reset graphics
            _overlay.Graphics.Clear();

            // Perform the query
            var queryParams = new QueryParameters()
            {
                Geometry = geometry,
                WhereClause = "1=1"
            };
            var table = hydrantsLayer.FeatureTable as ServiceFeatureTable;
            var results = await table.QueryFeaturesAsync(queryParams, QueryFeatureFields.LoadAll, CancellationToken.None);

            // Create new visuals and add to graphics overlay
            var resultsGraphics = results.Select(r => new Graphic(r.Geometry, new SimpleMarkerSymbol()
            {
                Color = Color.LimeGreen,
                Size = 12,
                Outline = new SimpleLineSymbol() { Color = Color.Green }
            })
            { ZIndex = 2 });
            var buffer = new Graphic(geometry, new SimpleFillSymbol()
            {
                Color = Color.Blue.MultiplyAlpha(0.5),
            })
            { ZIndex = 1 };

            _overlay.Graphics.Add(buffer);
            _overlay.Graphics.AddRange(resultsGraphics);

            // Update label with more details
            ResultsLabel.Text = $"Found {results.Count()} hydrants: ";
            ResultsLabel.Text += string.Join(", ", results.Select(r => r.Attributes["HYDRANT_NUM"]));
        }
    }
}
