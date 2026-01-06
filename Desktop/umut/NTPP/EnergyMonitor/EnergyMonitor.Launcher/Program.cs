using System;
using System.Windows.Forms;
using EnergyMonitor.Forms;

namespace EnergyMonitor.Launcher
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try 
            {
                while (true)
                {
                    // Show Login First
                    using (var frmLogin = new FrmLogin())
                    {
                        if (frmLogin.ShowDialog() == DialogResult.OK && frmLogin.GirisYapanKullanici != null)
                        {
                            // Pass the User to the Dashboard
                            Application.Run(new FrmDashboard(frmLogin.GirisYapanKullanici));
                            
                            // When Dashboard closes (e.g. LogOut), loop continues and shows Login again.
                        }
                        else
                        {
                            // User cancelled login or closed the login form
                            Application.Exit();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show("Kritik Hata: " + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace, "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}