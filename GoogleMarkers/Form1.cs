using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using GMap.NET.MapProviders;


namespace GoogleMarkers {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        public struct MapInfo
        {
            public RectLatLng Area;
            public int Zoom;
            public GMapProvider Type;
            public bool MakeWorldFile;
            public bool MakeKmz;//WE DONT USE IT BUT STRUCT NEEDS IT

            public MapInfo(RectLatLng Area, int Zoom, GMapProvider Type, bool makeWorldFile, bool MakeKmz)
            {
                this.Area = Area;
                this.Zoom = Zoom;
                this.Type = Type;
                this.MakeWorldFile = makeWorldFile;
                this.MakeKmz = MakeKmz;//WE DONT USE IT BUT STRUCT NEEDS IT
            }
        }

        private double _zoomFactor = 6;
        private string _markers = Directory.GetCurrentDirectory() + "/.tmp/_temporary/_markers";

        GMapOverlay markers_overlay = new GMapOverlay("markers");
        readonly List<GPoint> tileArea = new List<GPoint>();
        BackgroundWorker bg = new BackgroundWorker();
        TextBox tb_find_place;
        Button btn_find_place;
        Graphics gfx;

        private void InitUI() {
            btn_find_place = new Button {
                AutoSize = true,
                Text = "Find place",
                Location = new Point(mymap.Location.X, mymap.Location.Y + mymap.Height + 5)
            };
            btn_find_place.Click += Btn_find_place_Click;
            
            Controls.Add(btn_find_place);
            tb_find_place = new TextBox {
                Width = 250,
                Location = new Point(btn_find_place.Location.X + btn_find_place.Width + 10,
                                     btn_find_place.Location.Y + 2),
                Text = "Type destination",
            };
            Controls.Add(tb_find_place);
            tb_find_place.GotFocus += tb_find_place_GotFocus;
            tb_find_place.LostFocus += tb_find_place_LostFocus;
            //tb_find_place.KeyDown += Btn_find_place_Click;
        }

        private void tb_find_place_LostFocus(object sender, EventArgs e) {
            ((TextBox)sender).Text = ((TextBox)sender).Text == "" ? "Type destination" : ((TextBox)sender).Text;
        }

        private void tb_find_place_GotFocus(object sender, EventArgs e) {
            ((TextBox)sender).Text = ((TextBox)sender).Text == "Type destination" ? "" : ((TextBox)sender).Text;
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        private void Btn_find_place_Click(object sender, EventArgs e) {
            
            //if(sender == ((TextBox)sender)) {
            //    if (((KeyEventArgs)e).KeyCode != Keys.Return)
            //        return;
            //}

            var coords = GMap.NET.MapProviders.GMapProviders.GoogleMap.GetPoint(tb_find_place.Text, out GeoCoderStatusCode _e);
            if (coords.HasValue && _e.Equals(GeoCoderStatusCode.G_GEO_SUCCESS)) {
                mymap.SetPositionByKeywords(tb_find_place.Text);
                mymap.Zoom = 10;
            }
            else
            {
                string _s = FirstLetterToUpper(tb_find_place.Text);
                
                coords = GMap.NET.MapProviders.GMapProviders.GoogleMap.GetPoint(_s, out GeoCoderStatusCode __e);
                if (coords.HasValue && _e.Equals(GeoCoderStatusCode.G_GEO_SUCCESS))
                {
                    mymap.SetPositionByKeywords(tb_find_place.Text);
                    mymap.Zoom = 10;
                } else 
                    MessageBox.Show("Destination \"" + tb_find_place.Text + "\" could not be found.", "Bad destination request...", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void Form1_Load(object sender, EventArgs e) {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            WindowState = FormWindowState.Maximized;
            mymap.Width = Width - (3*mymap.Location.X);
            mymap.Height = Height - (4* mymap.Location.Y);

            mymap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;//using it as FULL reference to have the complete list of providers
            
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            mymap.SetPositionByKeywords("Greece");
            
            mymap.MinZoom = 0;
            mymap.MaxZoom = 18;
            mymap.Zoom = _zoomFactor;
            mymap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            mymap.DragButton = MouseButtons.Left;
            mymap.InvertedMouseWheelZooming = false;
            mymap.ShowCenter = false;



            CreateHiddenDir();
            LoadMarkers();
            InitUI();
        }

        private void PlaceMarker(MouseEventArgs _e) {
            if (_e.Button == MouseButtons.Left && !mymap.IsDragging) {

                PointLatLng final = new PointLatLng(
                        mymap.FromLocalToLatLng(_e.X, _e.Y).Lat,
                        mymap.FromLocalToLatLng(_e.X, _e.Y).Lng
                        );

                GMapMarker marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(final, GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green);
                marker.Tag = "Marker_" + markers_overlay.Markers.Count;
                markers_overlay.Markers.Add(marker);

                mymap.UpdateMarkerLocalPosition(marker);
                mymap.Overlays.Clear();
                mymap.Overlays.Add(markers_overlay);
            }
        }
        private void RemoveMarker(GMapMarker _item, MouseEventArgs _e) {
            if (_e.Button == MouseButtons.Right && !mymap.IsDragging) {
                if (mymap.Overlays.Count != 0) {
                    mymap.Overlays[0].Markers.Remove(_item);
                    markers_overlay = mymap.Overlays[0];
                    mymap.UpdateMarkerLocalPosition(_item);
                    mymap.Overlays.Clear();
                    mymap.Overlays.Add(markers_overlay);
                }
            }
        }
        private void SaveMarkers() {
            StreamWriter _wr;
            if (mymap.Overlays.Count != 0) {
                _wr = new StreamWriter(_markers);
                foreach (GMapMarker _m in mymap.Overlays[0].Markers) {
                    _wr.WriteLine(_m.Tag + "," + _m.Position.Lat + "," + _m.Position.Lng);
                }
                _wr.Close();
            }
        }
        private void LoadMarkers() {
            StreamReader _r;
            if (File.Exists(_markers)) {
                _r = new StreamReader(_markers);
                do {
                    string _tmp = _r.ReadLine();
                    PointLatLng final = new PointLatLng(
                        Convert.ToDouble(_tmp.Split(',')[1]),
                        Convert.ToDouble(_tmp.Split(',')[2])
                        );

                    GMapMarker marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(final, GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green);
                    marker.Tag = "Marker_" + markers_overlay.Markers.Count;
                    markers_overlay.Markers.Add(marker);
                    mymap.UpdateMarkerLocalPosition(marker);
                } while (!_r.EndOfStream);
                mymap.Overlays.Add(markers_overlay);
                mymap.SetZoomToFitRect(mymap.GetRectOfAllMarkers(mymap.Overlays[0].Id).Value);
                _r.Close();
            }
        }
        private void CreateHiddenDir() {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/.tmp/_temporary")) {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/.tmp/_temporary");
                DirectoryInfo dirinfo = new DirectoryInfo(Directory.GetCurrentDirectory() + "/.tmp");
                dirinfo.Attributes = FileAttributes.Hidden;
                dirinfo = new DirectoryInfo(Directory.GetCurrentDirectory() + "/.tmp/_temporary");
                dirinfo.Attributes = FileAttributes.Hidden;
            }
        }

        private void mymap_MouseClick(object sender, MouseEventArgs e) {
            PlaceMarker(e);
        }
        public static Bitmap takeComponentScreenShot(Control control)
        {
            Rectangle rect = control.RectangleToScreen(control.Bounds);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            return bmp;
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string bigImage = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "GMap_" + DateTime.Now.Ticks + ".png";
            Bitmap b = takeComponentScreenShot(mymap);
            b.Save(bigImage);
            if (File.Exists(bigImage)) MessageBox.Show("Picture saved to desktop!");
            else MessageBox.Show("Error while creating the image file");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            SaveMarkers();
        }

        private void mymap_OnMarkerClick(GMapMarker item, MouseEventArgs e) {
            RemoveMarker(item, e);
        }
    }
}
