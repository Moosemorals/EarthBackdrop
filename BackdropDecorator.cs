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

                int row = 1;
                for (int i = 0; i < DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month); i += 1) {
                    DateTime dayInMonth = startOfMonth.AddDays(i);

                    int col = ((7 + (int)dayInMonth.DayOfWeek - 1) % 7);
                    if (col == 0 && i > 0) {
                        row += 1;
                    }

                    float x = (glyphSize.Width + 2) * col;
                    float y = (glyphSize.Height + 2) * (row);

                    if (dayInMonth.Day == now.Day) {
                        gr.DrawRectangle(new Pen(Color.Red, 3), x, y, glyphSize.Width, glyphSize.Height);
                    }

                    string dayString = String.Format("{0}", dayInMonth.Day);

                    float rightAlignOffset = glyphSize.Width - gr.MeasureString(dayString, font).Width;

                    gr.DrawString(dayString, font, brush, x + rightAlignOffset, y);
                }

            }
        }

        private class DrawReportData {
            internal string Text { get; set; }
            internal SizeF Bounds { get; set; }

            internal DrawReportData(string text, Graphics gr, Font font) {
                Bounds = gr.MeasureString(text, font);
                Text = text;
            }
            internal DrawReportData(int value, string suffix, Graphics gr, Font font) {
                Text = string.Format("{0}{1}", value, suffix);
                Bounds = gr.MeasureString(Text, font);
            }
        }

        private static float DrawReport(Graphics gr, Report r, DateTime time, string timeFormat, Font font, Brush brush, float x, float y) {

            DrawReportData[] rows = new DrawReportData[] {
                new DrawReportData(time.ToString(timeFormat), gr, font),
                new DrawReportData(r.Glyph, gr, font),
                new DrawReportData(r.Temperature,"\uf042C", gr,font),
                new DrawReportData(r.PrecipitationProbibility, "%", gr, font),
                new DrawReportData(r.WindSpeed, "", gr, font),
                new DrawReportData(r.WindGust, "", gr, font),
                new DrawReportData(r.RelativeHumidity, "%", gr, font),
            };

            float width = 0;
            foreach (DrawReportData row in rows) {
                if (width < row.Bounds.Width) {
                    width = row.Bounds.Width;
                }
            }

            float offset = 0;
            foreach (DrawReportData row in rows) {
                float rightAlign = width - row.Bounds.Width;
                gr.DrawString(row.Text, font, brush, x + rightAlign, y + offset);
                offset += row.Bounds.Height + 2;

            }

            return width;
        }

        internal void AddWeather(HttpClient httpClient) {
            WeatherReport weather = new WeatherDownloader(httpClient).GetReport();

            DateTime now = DateTime.Now;
            using (PrivateFontCollection fonts = LoadWeatherIcons()) {
                using (Graphics gr = Graphics.FromImage(backdrop)) {
                    Brush brush = new SolidBrush(Color.White);
                    Font font = new Font(fonts.Families[0], 16);

                    float x = 0; 
                    float y = backdrop.Height - (font.Height * 10);
                    ReportEnumerator reports = new ReportEnumerator(weather);
                    int counter = 0;
                    foreach (Report r in reports) {
                        if (reports.ReportTime < now) {
                            continue;
                        }
                        float width = DrawReport(gr, r, reports.ReportTime, "HH:mm", font, brush, x, y);

                        x += width;
                        if (counter++ > 5) {
                            break;
                        }
                    }

                    reports.Reset();
                   
                    counter = 0;
                    x = backdrop.Width - x;
                    foreach(Report r in reports) {
                        if (reports.ReportTime < now || reports.ReportTime.Hour != 12) {
                            continue;
                        }

                        x += DrawReport(gr, r, reports.ReportTime, "ddd", font, brush, x, y);
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
