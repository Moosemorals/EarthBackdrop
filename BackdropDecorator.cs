using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                SizeF glyphSize  = gr.MeasureString("00", font);

                SizeF dateSize = gr.MeasureString(now.ToLongDateString(), font);

                gr.DrawString(now.ToLongDateString(), font, brush, ((glyphSize.Width * 7) - dateSize.Width) /2, 0);


                for (int i = 0; i < DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month); i += 1) {
                    DateTime dayInMonth = startOfMonth.AddDays(i);
                    float x = (glyphSize.Width + 2) * (int)dayInMonth.DayOfWeek;
                    float y = (glyphSize.Height + 2) * (1 + (dayInMonth.Day / 7));

                    if (dayInMonth.Day == now.Day) {
                        gr.DrawRectangle(new Pen(Color.Red, 3), x, y , glyphSize.Width, glyphSize.Height);
                    }

                    string dayString = String.Format("{0}", dayInMonth.Day);

                    gr.DrawString(dayString, font, brush, x + (glyphSize.Width - gr.MeasureString(dayString, font).Width), y); 
                }

            }
        }
    }
}
