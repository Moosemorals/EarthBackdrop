using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EarthBackdrop {
    internal class EarthDownloader {

        private readonly HttpClient httpClient;

        private readonly string imageURL;
        private const string IMAGEURL_KEY = "ImageURL";
        internal EarthDownloader(HttpClient httpClient) {
            this.httpClient = httpClient;
            imageURL = ConfigurationManager.AppSettings[IMAGEURL_KEY];
        }

        /// <summary>
        /// Fetch the image from the intranet
        /// </summary>
        /// <returns>A download task</returns>
        internal Image DownloadImage() {
            HttpResponseMessage resp = httpClient.GetAsync(imageURL).Result;
            if (resp.IsSuccessStatusCode) {
                return Image.FromStream(resp.Content.ReadAsStreamAsync().Result);
            }
            return null;
        }


    }
}
