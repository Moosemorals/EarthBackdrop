using EarthBackdrop.Weather;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EarthBackdrop {
    class BackdropDecorator {

        private readonly Bitmap backdrop;


        internal BackdropDecorator() {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            backdrop = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics gr = Graphics.FromImage(backdrop)) {
                gr.Clear(Color.Black);
            }
        }

        internal Image Backdrop {
            get {
                return backdrop;
            }
        }

        internal void AddImage(Image img) {
            // Adapted from https://stackoverflow.com/a/6565988/195833
            float rImg = (float)img.Width / (float)img.Height;
            float rScreen = (float)backdrop.Width / (float)backdrop.Height;

            var wNew = (rScreen > rImg) ? img.Width * backdrop.Height / img.Height : backdrop.Width;
            var hNew = (rScreen > rImg) ? backdrop.Height : img.Height * backdrop.Width / img.Width;

            Rectangle dest = new Rectangle {
                X = (backdrop.Width - wNew) / 2,
                Y = (backdrop.Height - hNew) / 2,
                Width = wNew,
                Height = hNew,
            };

            using (Graphics gr = Graphics.FromImage(backdrop)) {
                gr.DrawImage(img, dest);
            }
        }

        internal void AddCalendar(DateTime now) {
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);

            using (Graphics gr = Graphics.FromImage(backdrop)) {
                Brush brush = new SolidBrush(Color.White);
                Font font = new Font(FontFamily.GenericSansSerif, 16);

                SizeF glyphSize = gr.MeasureString("00", font);

                SizeF dateSize = gr.MeasureString(now.ToLongDateString(), font);

                gr.DrawString(now.ToLongDateString(), font, brush, ((glyphSize.Width * 7) - dateSize.Width) / 2, 0);


                for (int i = 0; i < DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month); i += 1) {
                    DateTime dayInMonth = startOfMonth.AddDays(i);
                    float x = (glyphSize.Width + 2) * (int)dayInMonth.DayOfWeek;
                    float y = (glyphSize.Height + 2) * (1 + (dayInMonth.Day / 7));

                    if (dayInMonth.Day == now.Day) {
                        gr.DrawRectangle(new Pen(Color.Red, 3), x, y, glyphSize.Width, glyphSize.Height);
                    }

                    string dayString = String.Format("{0}", dayInMonth.Day);

                    gr.DrawString(dayString, font, brush, x + (glyphSize.Width - gr.MeasureString(dayString, font).Width), y);
                }

            }
        }

        private static float WriteText(Graphics g, string text, Font font, Brush brush, float x, float y) {
            SizeF size = g.MeasureString(text, font);
            g.DrawString(text, font, brush, x, y);
            return size.Height;
        }

        internal void AddWeather(HttpClient httpClient) {
            WeatherReport weather = new WeatherDownloader(httpClient).GetReport();

            DateTime now = DateTime.Now; 
            using (PrivateFontCollection fonts = LoadWeatherIcons()) { 
                using (Graphics gr = Graphics.FromImage(backdrop)) {
                    Brush brush = new SolidBrush(Color.White);
                    Font font = new Font(fonts.Families[0], 16);

                    float x = 0;
                    ReportEnumerator reports = new ReportEnumerator(weather);
                    int counter = 0;
                    foreach (Report r in reports) {
                        if (reports.ReportTime < now) {
                            continue;
                        }

                        // Time
                        float y = backdrop.Height - (font.Height * 10);
                            
                        // Time 
                        y += WriteText(gr, reports.ReportTime.ToString("HH:mm"), font, brush, x, y); 

                        // Temperature
                        y += WriteText(gr, String.Format("{0,3}\uf042C", r.Temperature), font, brush, x, y);

                        // Wind - Speed
                        y += WriteText(gr, String.Format("{0,3}", r.WindSpeed), font, brush, x, y);

                        // Wind - Direction
                        y += WriteText(gr, String.Format("{0,3}", r.WindDirection), font, brush, x, y);

                        // Wind - Gust
                        y += WriteText(gr, String.Format("{0,3}", r.WindGust), font, brush, x, y);

                        // Rain percent
                        y += WriteText(gr, String.Format("{0,3}%", r.PrecipitationProbibility), font, brush, x, y);

                        // Humidity percent
                        y += WriteText(gr, String.Format("{0,3}%", r.RelativeHumidity), font, brush, x, y);

                        // General Weather
                        y += WriteText(gr, String.Format("{0, 3}", r.Glyph), font, brush, x, y);

                        x += gr.MeasureString("00:00 ", font).Width;
                        if (counter++ > 5) {
                            break;
                        }
                    }
                }
            }
        }

        private static PrivateFontCollection LoadWeatherIcons() {
            // Adapted from https://stackoverflow.com/a/23520042/195833 
            PrivateFontCollection fonts = new PrivateFontCollection();

            int fontLength = Properties.Resources.weathericons_regular_webfont.Length;
            byte[] fontData = Properties.Resources.weathericons_regular_webfont;

            IntPtr data = Marshal.AllocCoTaskMem(fontLength);
            Marshal.Copy(fontData, 0, data, fontLength);

            fonts.AddMemoryFont(data, fontLength);

            return fonts;
        }
    }
}
