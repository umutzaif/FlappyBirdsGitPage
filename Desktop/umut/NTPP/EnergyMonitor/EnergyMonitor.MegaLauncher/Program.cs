using System;
using System.Windows.Forms;

namespace EnergyMonitor.MegaLauncher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FrmMegaLauncher());
        }
    }
}