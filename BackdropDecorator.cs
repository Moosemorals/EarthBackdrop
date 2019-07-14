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
    }
}
