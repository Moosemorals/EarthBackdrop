using EarthBackdrop.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EarthBackdrop {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EarthBackdropApplicationContext());
        }
    }

    public class EarthBackdropApplicationContext : ApplicationContext {
        private readonly NotifyIcon trayIcon;
        private readonly EarthDownloader downloader;

        public EarthBackdropApplicationContext() {

            downloader = new EarthDownloader();
            downloader.Start();

            trayIcon = new NotifyIcon() {
                Icon = Icon.FromHandle( Resources.AppIcon.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e) {

            downloader.Stop();

            trayIcon.Visible = false;
            Application.Exit();
        }

    }
}
