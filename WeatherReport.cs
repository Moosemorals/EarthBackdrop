using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthBackdrop.Weather {


    public class ReportWrapper {
        [JsonProperty("SiteRep")]
        public WeatherReport SiteReport { get; set; }
    }

    public class WeatherReport {
        [JsonProperty("Wx")]
        public Meta Meta { get; set; }
        [JsonProperty("DV")]
        public Data Data { get; set; }

        public static DateTime GetReportTime(Period p, Report r) {
            return p.Date.AddMinutes(r.OffsetMinutes);
        }

        public static int MphToBeaufort(int speed) {
            if (speed < 1) {
                return 0;
            } else if (speed < 4) {
                return 1;
            } else if (speed < 8) {
                return 2;
            } else if (speed < 13) {
                return 3;
            } else if (speed < 19) {
                return 4;
            } else if (speed < 25) {
                return 5;
            } else if (speed < 32) {
                return 6;
            } else if (speed < 39) {
                return 7;
            } else if (speed < 47) {
                return 8;
            } else if (speed < 55) {
                return 9;
            } else if (speed < 64) {
                return 10;
            } else if (speed < 73) {
                return 11;
            } else {
                return 12;
            }
        }

        public static int WindDirToDegrees(string dir) {
            switch (dir) {
                case "N":
                    return 0;
                case "NNE":
                    return 23;
                case "NE":
                    return 45;
                case "ENE":
                    return 68;
                case "E":
                    return 90;
                case "ESE":
                    return 113;
                case "SE":
                    return 135;
                case "SSE":
                    return 158;
                case "S":
                    return 180;
                case "SSW":
                    return 203;
                case "SW":
                    return 225;
                case "WSW":
                    return 248;
                case "W":
                    return 270;
                case "WNW":
                    return 293;
                case "NW":
                    return 315;
                case "NNW":
                    return 338;
                default:
                    return 0;
            }
        }

        public static readonly IDictionary<string, WeatherType> WeatherTypes = new Dictionary<string, WeatherType>() {
            { "NA", new WeatherType("Not availible", "\uf07b") },
            { "0", new WeatherType("Clear", "\uf02e") },
            { "1", new WeatherType("Sunny", "\uf00d") },
            { "2", new WeatherType("Partly cloudy", "\uf031") },
            { "3", new WeatherType("Partly cloudy", "\uf002") },
            { "5", new WeatherType("Mist", "\uf014") },
            { "6", new WeatherType("Fog", "\uf014") },
            { "7", new WeatherType("Cloudy", "\uf013") },
            { "8", new WeatherType("Overcast", "\uf041") },
            { "9", new WeatherType("Light rain shower", "\uf037") },
            { "10", new WeatherType("Light rain shower", "\uf009") },
            { "11", new WeatherType("Drizzle", "\uf017") },
            { "12", new WeatherType("Light Rain", "\uf01a") },
            { "13", new WeatherType("Heavy rain shower", "\uf036") },
            { "14", new WeatherType("Heavy rain shower", "\uf008") },
            { "15", new WeatherType("Heavy rain", "\uf019") },
            { "16", new WeatherType("Sleet shower", "\uf0b3") },
            { "17", new WeatherType("Sleet shower", "\uf0b2") },
            { "18", new WeatherType("Sleet", "\uf0b5") },
            { "19", new WeatherType("Hail shower", "\uf032") },
            { "20", new WeatherType("Hail shower", "\uf004") },
            { "21", new WeatherType("Hail", "\uf015") },
            { "22", new WeatherType("Light snow shower", "\uf038") },
            { "23", new WeatherType("Light snow shower", "\uf000") },
            { "24", new WeatherType("Light snow", "\uf01b") },
            { "25", new WeatherType("Heavy snow shower", "\uf038") },
            { "26", new WeatherType("Heavy snow shower", "\uf000") },
            { "27", new WeatherType("Heavy snow", "\uf01b") },
            { "28", new WeatherType("Thunder shower", "\uf03b") },
            { "29", new WeatherType("Thunder shower", "\uf010") },
            { "30", new WeatherType("Thunder", "\uf01e") },
        };
    }
    internal class ReportEnumerator : IEnumerator, IEnumerable {
        private readonly WeatherReport weatherReport;
        private Period currentPeriod;
        private int perioidCounter = 0;
        private Report currentReport;
        private int reportCounter = 0;

        public ReportEnumerator(WeatherReport weatherReport) {
            this.weatherReport = weatherReport;
            this.currentPeriod = this.weatherReport.Data.Location.Period[0];
            this.currentReport = this.currentPeriod.Reports[0];
        }

        public Report Current => this.currentReport;

        object IEnumerator.Current => this.currentReport;

        public void Dispose() {
            // does nothing
        }

        public IEnumerator GetEnumerator() {
            return this;
        }

        public bool MoveNext() {
            reportCounter += 1;
            if (reportCounter >= currentPeriod.Reports.Length) {
                perioidCounter += 1;
                if (perioidCounter >= weatherReport.Data.Location.Period.Length) {
                    return false;
                }
                currentPeriod = weatherReport.Data.Location.Period[perioidCounter];
                reportCounter = 0;
            }
            currentReport = currentPeriod.Reports[reportCounter];
            return true;
        }

        public void Reset() {
            reportCounter = 0;
            perioidCounter = 0;
            currentPeriod = weatherReport.Data.Location.Period[0];
            currentReport = currentPeriod.Reports[0];
        }

        public DateTime ReportTime {
            get {
                return WeatherReport.GetReportTime(currentPeriod, currentReport);
            }
        }

    }

    public class Meta {
        public Param[] Param { get; set; }
    }

    public class Param {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("units")]
        public string Units { get; set; }

        [JsonProperty("$")]
        public string Text { get; set; }
    }

    public class Data {
        [JsonProperty("dataDate")]
        public DateTime Date { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public Location Location { get; set; }
    }

    public class Location {
        [JsonProperty("i")]
        public int ID { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longittude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("conntry")]
        public string Country { get; set; }

        [JsonProperty("continent")]
        public string Continent { get; set; }

        [JsonProperty("elevation")]
        public float Elevation { get; set; }

        public Period[] Period { get; set; }
    }

    public class Period {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public DateTime Date { get; set; }

        [JsonProperty("Rep")]
        public Report[] Reports { get; set; }
    }

    public class Report {
        [JsonProperty("F")]
        public int FeelsLike { get; set; }

        [JsonProperty("G")]
        public int WindGust { get; set; }

        [JsonProperty("H")]
        public int RelativeHumidity { get; set; }

        [JsonProperty("T")]
        public int Temperature { get; set; }

        [JsonProperty("V")]
        public string Visibility { get; set; }

        [JsonProperty("D")]
        public string WindDirection { get; set; }

        [JsonProperty("S")]
        public int WindSpeed { get; set; }

        [JsonProperty("U")]
        public int MaxUVIndex { get; set; }

        [JsonProperty("W")]
        public string WeatherType { get; set; }

        [JsonProperty("Pp")]
        public int PrecipitationProbibility { get; set; }

        [JsonProperty("$")]
        public int OffsetMinutes { get; set; }

        [JsonIgnore]
        public string Glyph {
            get {
                return WeatherReport.WeatherTypes[WeatherType].Glyph;
            }
        }

        [JsonIgnore]
        public string WeatherText {
            get {
                return WeatherReport.WeatherTypes[WeatherType].Description;
            }
        }
    }

    public class WeatherType {
        public WeatherType(string description, string glyph) {
            Description = description;
            Glyph = glyph;
        }

        public string Description { get; }
        public string Glyph { get; }

    }
}
