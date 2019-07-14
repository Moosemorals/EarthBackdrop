using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EarthBackdrop {
    class EarthDownloader {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        // TODO: Load this from some kind of config repository
        private const string TARGET_URL = "https://eumetview.eumetsat.int/static-images/latestImages/EUMETSAT_MSG_RGBNatColourEnhncd_FullResolution.jpg";
        private static readonly HttpClient client = new HttpClient();

        private readonly object Lock = new object();
        private bool running = false;
        private EarthBackdropApplicationContext earthBackdropApplicationContext;

        public EarthDownloader(EarthBackdropApplicationContext earthBackdropApplicationContext) {
            this.earthBackdropApplicationContext = earthBackdropApplicationContext;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        /// <summary>
        /// Start a new download thread (if we're not already running)
        /// </summary>
        internal void Start() {
            lock (Lock) {
                if (running) {
                    return;
                }
                running = true;
                new Thread(() => {
                    Thread.CurrentThread.IsBackground = true;
                     Run();
                }).Start();

            }
        }

        /// <summary>
        /// Wake up the download thread
        /// </summary>
        internal void Refresh() {
            lock(Lock) {
                Monitor.Pulse(Lock);
            }
        }

        /// <summary>
        /// Download the image, then wait for an hour (or untill we're woken up again)
        /// </summary>
        private void Run() {
            while (true) {
                lock (Lock) {
                    if (!running) {
                        return;
                    }
                    var img = DownloadImage();
                    if (img != null) {
                        Image earth = img.Result;
                        string msg = String.Format("Last download was at {0}", DateTime.Now.ToString("f"));
                        SetBackground(earth);
                        earthBackdropApplicationContext.UpdateTrayIcon(earth, msg);
                    }
                    Monitor.Wait(Lock, TimeSpan.FromHours(1));
                }          
            }
        }

        /// <summary>
        /// Fetch the image from the intranet
        /// </summary>
        /// <returns>A download task</returns>
        private async Task<Image> DownloadImage() {
            HttpResponseMessage resp = await client.GetAsync(TARGET_URL);
            if (resp.IsSuccessStatusCode) {
                return Image.FromStream(await resp.Content.ReadAsStreamAsync());
            }
            return null;
        }

        /// <summary>
        /// Set the desktop background. The image passed is saved as a bitmap
        /// to a temporary path (from Path.GetTempPath()) first.
        /// </summary>
        /// <param name="img">The image data to use for the background</param>
        private void SetBackground(Image img) {
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", "6"); //Fit
            key.SetValue(@"TileWallpaper", "0");
            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        /// <summary>
        /// Tells the download thread to stop (if it's running)
        /// </summary>
        internal void Stop() {
            lock (Lock) {
                if (running) {
                    running = false;
                    Monitor.Pulse(Lock);
                }
            }
        }
    }
}
