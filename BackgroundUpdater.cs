using Microsoft.Win32;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EarthBackdrop {
    internal class BackgroundUpdater {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private readonly HttpClient httpClient;

        private readonly object Lock = new object();
        private bool running = false;
        private readonly EarthBackdropApplicationContext parent;

        internal BackgroundUpdater(EarthBackdropApplicationContext earthBackdropApplicationContext) {
            parent = earthBackdropApplicationContext;
            httpClient = parent.httpClient;
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
            lock (Lock) {
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
                    Image earth = new EarthDownloader(httpClient).DownloadImage();
                    if (earth != null) {
                        string msg = String.Format("Last download was at {0}", DateTime.Now.ToString("f"));
                        SetBackground(DecorateBackdrop(earth));
                        parent.UpdateTrayIcon(earth, msg);
                    }
                    Monitor.Wait(Lock, TimeSpan.FromHours(1));
                }
            }
        }

        private Image DecorateBackdrop(Image earth) {
            BackdropDecorator d = new BackdropDecorator();
            d.AddImage(earth);
            d.AddCalendar(DateTime.Now);
            d.AddWeather(httpClient);
            return d.Backdrop;
        }

        /// <summary>
        /// Set the desktop background. The image passed is saved as a bitmap
        /// to a temporary path (from Path.GetTempPath()) first.
        /// </summary>
        /// <param name="img">The image data to use for the background</param>
        private void SetBackground(Image img) {
            // Adapted from https://stackoverflow.com/a/1061682/195833
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            Console.WriteLine("Saving background to {0}", tempPath);
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", "0"); // Center
            key.SetValue(@"TileWallpaper", "0");
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
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
