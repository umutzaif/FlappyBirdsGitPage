using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace EnergyMonitor.MegaLauncher
{
    public class FrmMegaLauncher : Form
    {
        private WebView2 webView;
        private bool servislerBaslatildi = false;

        private string videoYolu = @"C:\Users\Asus-PC\Downloads\Blue and White Modern Login and Sign-up Website Page UI Desktop Prototype.mp4";

        public FrmMegaLauncher()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Enerlytics MegaLauncher";
            
            // Splash Screen Dimensions (16:9 professional scale)
            int splashWidth = 800;
            int splashHeight = 450;
            
            this.Size = new Size(splashWidth, splashHeight);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false; // Hide from taskbar for a cleaner look
            this.TopMost = true;

            webView = new WebView2 
            { 
                Dock = DockStyle.Fill 
            };
            
            this.Controls.Add(webView);
            this.Load += FrmMegaLauncher_Load;
        }

        private async void FrmMegaLauncher_Load(object sender, EventArgs e)
        {
            this.Opacity = 0; // Geçici olarak gizle
            DurumuGuncelle("Servisler başlatılıyor...");
            await TumServisleriBaslat();
            
            await Task.Delay(3000); // Wait for terminals to appear
            this.Opacity = 1;
            this.TopMost = true;

            try 
            {
                await webView.EnsureCoreWebView2Async();
                webView.DefaultBackgroundColor = Color.Black;

                string downloadsFolder = Path.GetDirectoryName(videoYolu);
                string fileName = Path.GetFileName(videoYolu);
                
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "enerlytics.host", 
                    downloadsFolder, 
                    CoreWebView2HostResourceAccessKind.Allow);

                webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                
                string encodedFileName = Uri.EscapeDataString(fileName);
                string html = $@"
                    <html>
                    <body style='margin:0; padding:0; background:black; overflow:hidden;'>
                        <video id='bgVideo' width='100%' height='100%' autoplay muted style='object-fit:cover;'>
                            <source src='https://enerlytics.host/{encodedFileName}' type='video/mp4'>
                        </video>
                        <script>
                            var video = document.getElementById('bgVideo');
                            video.onended = function() {{
                                window.chrome.webview.postMessage('videoEnded');
                            }};
                            video.onerror = function() {{
                                window.chrome.webview.postMessage('videoError');
                            }};
                        </script>
                    </body>
                    </html>";
                
                webView.NavigateToString(html);
                webView.WebMessageReceived += (s, args) => {
                    string msg = args.TryGetWebMessageAsString();
                    if (msg == "videoEnded") AnaUygulamayiBaslat();
                    if (msg == "videoError") {
                         AnaUygulamayiBaslat();
                    }
                };

                // StartAllServices exceeded here (already called at top of Load)
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Video Hatası: " + ex.Message);
                AnaUygulamayiBaslat();
            }
        }

        private async Task TumServisleriBaslat()
        {
            if (servislerBaslatildi) return;
            servislerBaslatildi = true;

            string slnRoot = CozumKokDizininiBul();
            
            DurumuGuncelle("MQTT Broker Başlatılıyor...");
            SureciBaslat(slnRoot, "EnergyMonitor.Broker", "EnergyMonitor.Broker.exe");
            await Task.Delay(1000);

            DurumuGuncelle("Analiz API Başlatılıyor...");
            SureciBaslat(slnRoot, "EnergyMonitor.Api", "EnergyMonitor.Api.exe");
            await Task.Delay(1000);

            DurumuGuncelle("Simülatör Başlatılıyor...");
            SureciBaslat(slnRoot, "MockEsp32", "MockEsp32.exe");
            await Task.Delay(1000);

            DurumuGuncelle("Sistem Hazır.");
        }

        private void SureciBaslat(string slnRoot, string projectDir, string exeName)
        {
            try 
            {
                // Birkaç yaygın konumu dene
                string[] paths = {
                    Path.Combine(slnRoot, projectDir, "bin", "Debug", "net8.0", exeName),
                    Path.Combine(slnRoot, projectDir, "bin", "Debug", "net8.0-windows", exeName),
                    Path.Combine(slnRoot, projectDir, exeName) // Proje kökü (orada derlendiyse)
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        Process.Start(new ProcessStartInfo {
                            FileName = path,
                            WorkingDirectory = Path.GetDirectoryName(path),
                            UseShellExecute = true,
                            CreateNoWindow = false
                        });
                        return;
                    }
                }
                Debug.WriteLine($"{exeName} bulunamadı.");
            }
            catch (Exception ex) { Debug.WriteLine($"{exeName} başlatılırken hata oluştu: {ex.Message}"); }
        }

        private void AnaUygulamayiBaslat()
        {
            string slnRoot = CozumKokDizininiBul();
            string launcherPath = Path.Combine(slnRoot, "EnergyMonitor.Launcher", "bin", "Debug", "net8.0-windows", "EnergyMonitor.Launcher.exe");
            
            if (File.Exists(launcherPath))
            {
                Process.Start(new ProcessStartInfo {
                    FileName = launcherPath,
                    WorkingDirectory = Path.GetDirectoryName(launcherPath),
                    UseShellExecute = true
                });
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Ana uygulama bulunamadı: " + launcherPath);
                Application.Exit();
            }
        }

        private void DurumuGuncelle(string metin)
        {
            Debug.WriteLine($"[MegaLauncher] {metin}");
        }

        private string CozumKokDizininiBul()
        {
            string current = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(current))
            {
                if (File.Exists(Path.Combine(current, "EnergyMonitor.sln"))) return current;
                current = Path.GetDirectoryName(current);
            }
            return @"C:\Users\Asus-PC\Desktop\umut\NTPP\EnergyMonitor"; // Fallback
        }
    }
}
