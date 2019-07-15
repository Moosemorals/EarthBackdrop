using EarthBackdrop.Properties;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;

namespace EarthBackdrop {



    static class Startup {

        private static readonly HttpClient client = new HttpClient();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EarthBackdropApplicationContext(client));
        }
    }

    public class EarthBackdropApplicationContext : ApplicationContext {
        private readonly NotifyIcon trayIcon;
        private readonly BackgroundUpdater downloader;
        internal readonly HttpClient httpClient;

        public EarthBackdropApplicationContext(HttpClient httpClient) {
            this.httpClient = httpClient;

            trayIcon = new NotifyIcon() {
                Icon = Icon.FromHandle(Resources.AppIcon.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Refresh", Refresh ),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            downloader = new BackgroundUpdater(this);
            downloader.Start();

            SystemEvents.PowerModeChanged += OnPowerModeChanged; 
        }

        internal void UpdateTrayIcon(Image source,string msg){
            trayIcon.Icon = IconFromImage(source);
            trayIcon.Text = msg;
        }


        // Copied from https://stackoverflow.com/a/21389253/195833
        private static Icon IconFromImage(Image img) {
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            // Header
            bw.Write((short)0);   // 0 : reserved
            bw.Write((short)1);   // 2 : 1=ico, 2=cur
            bw.Write((short)1);   // 4 : number of images
                                  // Image directory
            var w = img.Width;
            if (w >= 256) w = 0;
            bw.Write((byte)w);    // 0 : width of image
            var h = img.Height;
            if (h >= 256) h = 0;
            bw.Write((byte)h);    // 1 : height of image
            bw.Write((byte)0);    // 2 : number of colors in palette
            bw.Write((byte)0);    // 3 : reserved
            bw.Write((short)0);   // 4 : number of color planes
            bw.Write((short)0);   // 6 : bits per pixel
            var sizeHere = ms.Position;
            bw.Write((int)0);     // 8 : image size
            var start = (int)ms.Position + 4;
            bw.Write(start);      // 12: offset of image data
                                  // Image data
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var imageSize = (int)ms.Position - start;
            ms.Seek(sizeHere, System.IO.SeekOrigin.Begin);
            bw.Write(imageSize);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // And load it
            return new Icon(ms);
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
            switch (e.Mode) {
                case PowerModes.Resume:
                    downloader.Refresh();
                    break;
            }
        }

        void Exit(object sender, EventArgs e) {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;

            downloader.Stop();
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void Refresh(object sender, EventArgs e) {
            downloader.Refresh();
        }

    }
}
