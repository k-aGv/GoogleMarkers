using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace GoogleMarkers {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private double _zoomFactor = 6;
        GMapOverlay markers_overlay = new GMapOverlay("markers");
        List<GMapMarker> markers = new List<GMapMarker>();


        private void Form1_Load(object sender, EventArgs e) {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            WindowState = FormWindowState.Maximized;
            mymap.Width = Width - (3*mymap.Location.X);
            mymap.Height = Height - (5* mymap.Location.Y);

            mymap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;//using it as FULL reference to have the complete list of providers
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            mymap.SetPositionByKeywords("Greece");
            
            mymap.MinZoom = 0;
            mymap.MaxZoom = 18;
            mymap.Zoom = _zoomFactor;
            mymap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mymap.DragButton = MouseButtons.Left;
            mymap.InvertedMouseWheelZooming = false;
        }

        private void PlaceMarker(MouseEventArgs _e) {
            PointLatLng final = new PointLatLng(
                    mymap.FromLocalToLatLng(_e.X, _e.Y).Lat,
                    mymap.FromLocalToLatLng(_e.X, _e.Y).Lng
                    );

            GMapMarker marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(final, GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green);
            markers.Add(marker);
            markers_overlay.Markers.Add(markers[markers.Count - 1]);

            mymap.UpdateMarkerLocalPosition(markers[markers.Count - 1]);
            mymap.Overlays.Add(markers_overlay);
        }

        private void mymap_MouseClick(object sender, MouseEventArgs e) {
            PlaceMarker(e);
        }
    }
}
