using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EarthBackdrop {
    class WeatherDownloader {

        private const string TARGET_URL = "http://datapoint.metoffice.gov.uk/public/data/val/wxfcs/all/json/352790?res=3hourly&key=";
        private readonly string API_KEY;
        private readonly HttpClient httpClient;

        internal WeatherDownloader(HttpClient httpClient) {
            this.httpClient = httpClient;
            this.API_KEY = ConfigurationManager.AppSettings["MetOfficeApiKey"];
        }

        internal Weather.WeatherReport GetReport() {
            var downloadTask = httpClient.GetAsync(GetReportUrl());
            HttpResponseMessage resp = downloadTask.Result;
            if (resp.IsSuccessStatusCode) {
                string raw = resp.Content.ReadAsStringAsync().Result;
                Weather.ReportWrapper r = JsonConvert.DeserializeObject<Weather.ReportWrapper>(raw);
                return r.SiteReport;
            }
            return null;
        }


        private string GetReportUrl() {
            return TARGET_URL + API_KEY;
        }
    }
}
