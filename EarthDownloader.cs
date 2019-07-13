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
        private static readonly HttpClient client = new HttpClient();
        private const string TARGET_URL = "https://eumetview.eumetsat.int/static-images/latestImages/EUMETSAT_MSG_RGBNatColourEnhncd_FullResolution.jpg";
        private bool running = false;
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        internal void Start() {

            running = true;
            new Thread(async () => {
                Thread.CurrentThread.IsBackground = true;
                await Run();
            }).Start();
        }
        

        private async Task Run() {
            while (running) {
                var img = await DownloadImage();
                SetBackground(img);
                Sleep();
            }
        }

        private async Task<Image> DownloadImage() {
            HttpResponseMessage resp = await client.GetAsync(TARGET_URL);
            if (resp.IsSuccessStatusCode) {
                return Image.FromStream(await resp.Content.ReadAsStreamAsync());
            }
            return null;
        }

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

        private void Sleep() {
            Thread.Sleep(TimeSpan.FromMinutes(60));
        }
        internal void Stop() {
            running = false;
        }
    }
}
