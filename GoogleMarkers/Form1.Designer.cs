namespace GoogleMarkers {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.mymap = new GMap.NET.WindowsForms.GMapControl();
            this.SuspendLayout();
            // 
            // mymap
            // 
            this.mymap.Bearing = 0F;
            this.mymap.CanDragMap = true;
            this.mymap.EmptyTileColor = System.Drawing.Color.Navy;
            this.mymap.GrayScaleMode = false;
            this.mymap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.mymap.LevelsKeepInMemmory = 5;
            this.mymap.Location = new System.Drawing.Point(12, 12);
            this.mymap.MarkersEnabled = true;
            this.mymap.MaxZoom = 2;
            this.mymap.MinZoom = 2;
            this.mymap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.mymap.Name = "mymap";
            this.mymap.NegativeMode = false;
            this.mymap.PolygonsEnabled = true;
            this.mymap.RetryLoadTile = 0;
            this.mymap.RoutesEnabled = true;
            this.mymap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.mymap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.mymap.ShowTileGridLines = false;
            this.mymap.Size = new System.Drawing.Size(669, 426);
            this.mymap.TabIndex = 0;
            this.mymap.Zoom = 0D;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 522);
            this.Controls.Add(this.mymap);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private GMap.NET.WindowsForms.GMapControl mymap;
    }
}

