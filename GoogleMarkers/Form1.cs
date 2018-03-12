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

        private void Form1_Load(object sender, EventArgs e) {

            mymap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;//using it as FULL reference to have the complete list of providers
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            mymap.SetPositionByKeywords("greece,thessaloniki");
            mymap.MinZoom = 0;
            mymap.MaxZoom = 18;
            //mymap.Zoom = _zoomFactor;
            mymap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mymap.DragButton = MouseButtons.Left;
            mymap.InvertedMouseWheelZooming = false;
        }
    }
}
