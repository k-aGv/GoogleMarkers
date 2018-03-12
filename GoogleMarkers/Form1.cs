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

        Graphics gfx;


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



            CreateHiddenDir();
            LoadMarkers();
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

       
        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bg.WorkerReportsProgress = true;
            bg.WorkerSupportsCancellation = true;
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);
            bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
            RectLatLng AreaGpx = RectLatLng.Empty;
            DialogResult save = MessageBox.Show("Save?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (save == DialogResult.No) return;

            RectLatLng? area; // abstract declaration to allow multithreading process access it
            area = new RectLatLng(mymap.ViewArea.Lat
                ,mymap.ViewArea.Lng
                ,mymap.ViewArea.WidthLng
                ,mymap.ViewArea.HeightLat);
                      
            int currentZoom = Convert.ToInt32( mymap.Zoom);
            if (!bg.IsBusy)
            {
                lock (tileArea)
                {
                    tileArea.Clear();
                    tileArea.AddRange(mymap.MapProvider.Projection.GetAreaTileList(area.Value, currentZoom, 1));
                    tileArea.TrimExcess();
                }
                bg.RunWorkerAsync(new MapInfo(area.Value, currentZoom, mymap.MapProvider, false, false));
                mymap.Refresh();
            }
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                    MessageBox.Show("Error:" + e.Error.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

         
            mymap.Refresh();

        }

        void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GPoint p = (GPoint)e.UserState;
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            MapInfo info = (MapInfo)e.Argument;
            if (!info.Area.IsEmpty)
            {
                string bigImage = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + "GMap_" + info.Type + "_" + DateTime.Now.Ticks + ".png";
                e.Result = bigImage;

                // current area
                GPoint topLeftPx = info.Type.Projection.FromLatLngToPixel(info.Area.LocationTopLeft, info.Zoom);
                GPoint rightButtomPx = info.Type.Projection.FromLatLngToPixel(info.Area.Bottom, info.Area.Right, info.Zoom);
                GPoint pxDelta = new GPoint(rightButtomPx.X - topLeftPx.X, rightButtomPx.Y - topLeftPx.Y);
                GSize maxOfTiles = info.Type.Projection.GetTileMatrixMaxXY(info.Zoom);

                int padding = info.MakeWorldFile || info.MakeKmz ? 0 : 22;
                {
                    //gfx = Graphics.FromImage(bmpDe)
                    Bitmap bmpDestination = new Bitmap((int)(pxDelta.X + padding * 2), (int)(pxDelta.Y + padding * 2));
                    gfx = Graphics.FromImage(bmpDestination);

                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.SmoothingMode = SmoothingMode.HighQuality;
                    gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
               
                    int i = 0;

                    // get tiles & combine into one
                    lock (tileArea)
                    {
                        foreach (var p in tileArea)
                        {
                            if (bg.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }

                            int pc = (int)(((double)++i / tileArea.Count) * 100);
                            bg.ReportProgress(pc, p);
                            
                            foreach (var tp in info.Type.Overlays)
                            {
                                Exception ex;
                                GMapImage tile;

                                // tile number inversion(BottomLeft -> TopLeft) for pergo maps
                                if (tp.InvertedAxisY)
                                {
                                    tile = GMaps.Instance.GetImageFrom(tp, new GPoint(p.X, maxOfTiles.Height - p.Y), info.Zoom, out ex) as GMapImage;
                                }
                                else // ok
                                {
                                    tile = GMaps.Instance.GetImageFrom(tp, p, info.Zoom, out ex) as GMapImage;
                                }

                                if (tile != null)
                                {
                                    using (tile)
                                    {
                                        long x = p.X * info.Type.Projection.TileSize.Width - topLeftPx.X + padding;
                                        long y = p.Y * info.Type.Projection.TileSize.Width - topLeftPx.Y + padding;
                                        {
                                            gfx.DrawImage(tile.Img, x, y, info.Type.Projection.TileSize.Width, info.Type.Projection.TileSize.Height);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // draw info
                    if (!info.MakeWorldFile)
                    {
                        Rectangle rect = new System.Drawing.Rectangle();
                        {
                            rect.Location = new Point(padding, padding);
                            rect.Size = new Size((int)pxDelta.X, (int)pxDelta.Y);
                        }

                        bmpDestination.Save(bigImage, ImageFormat.Jpeg);
                    }
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            SaveMarkers();
        }
    }
}
