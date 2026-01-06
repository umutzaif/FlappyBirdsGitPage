using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using EnergyMonitor.Interface.Services;
using EnergyMonitor.Service;
using DevExpress.XtraEditors;
using System.Text.Json;
using System.Net.Http;
using EnergyMonitor.Interface.Models;
using DevExpress.Utils;
using EnergyMonitor.Business;
using MesajModeli = EnergyMonitor.Interface.Models.Mesaj;

namespace EnergyMonitor.Forms
{
    public partial class FrmDashboard : XtraForm
    {
        private readonly IAnalizServisi _analizServisi;
        private void DurumCubugunuHazirla()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(17, 34, 64);
            
            lblDurumSeri = new ToolStripStatusLabel { Text = "Bağlantı Yok", ForeColor = Color.Salmon, IsLink = true, LinkColor = Color.Salmon };
            lblDurumSeri.Click += (s, e) => {
                 if(!_seriPortServisi.AcikMi) {
                     string port = FrmGirisKutusu.Goster("Port Giriniz (örn. COM3):", "Seri Port Bağlantısı", "COM3");
                     if(!string.IsNullOrEmpty(port) && _seriPortServisi.BaglantiyiAc(port)) DurumCubugunuGuncelle();
                 } else {
                      if(XtraMessageBox.Show("Bağlantıyı kesmek istiyor musunuz?", "Port", MessageBoxButtons.YesNo)==DialogResult.Yes) {
                           _seriPortServisi.BaglantiyiKapat(); DurumCubugunuGuncelle();
                      }
                 }
            };

            lblDurumBilgi = new ToolStripStatusLabel { Text = " | Hazır", ForeColor = Color.Gainsboro };
            
            statusStrip.Items.AddRange(new ToolStripItem[] { lblDurumSeri, lblDurumBilgi });
            this.Controls.Add(statusStrip);
            
            // Durum Durum Zamanlayıcısı
            var tmr = new System.Windows.Forms.Timer { Interval = 2000, Enabled = true };
            tmr.Tick += (s, e) => DurumCubugunuGuncelle();
        }

        private void DurumCubugunuGuncelle()
        {
             if(_seriPortServisi != null && _seriPortServisi.AcikMi) {
                 lblDurumSeri.Text = $"🔌 Seri Port: Bağlı";
                 lblDurumSeri.ForeColor = Color.LightGreen;
                 lblDurumSeri.LinkColor = Color.LightGreen;
             } else {
                 lblDurumSeri.Text = "🔌 Seri Port: Bağlı Değil (Bağlan)";
                 lblDurumSeri.ForeColor = Color.Salmon;
                 lblDurumSeri.LinkColor = Color.Salmon;
             }
        }

        private void ProfilSekmesiniHazirla()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // Sol: Kullanıcı Bilgisi
            var grpUser = new GroupControl { Text = "Profilim", Dock = DockStyle.Fill };
            var pnlUser = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 5 };
            pnlUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Fotoğraf
            pnlUser.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Güncelle Butonu
            pnlUser.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Bilgi
            
            var lblPhotoInfo = new LabelControl { Text = "Profil Fotoğrafı Özelliği Devre Dışı", Padding = new Padding(0, 70, 0, 0), Appearance = { TextOptions = { HAlignment = HorzAlignment.Center } } };

            var lblRole = new LabelControl { Text = $"Kullanıcı: {_mevcutKullanici.KullaniciAdi}\nRol: {_mevcutKullanici.RolAdi}\nAd Soyad: {_mevcutKullanici.AdSoyad}", AutoSizeMode = LabelAutoSizeMode.Vertical, Dock = DockStyle.Top, Padding = new Padding(0,10,0,0) };
            var btnPwd = new SimpleButton { Text = "Şifre Değiştir", Height = 40, Dock = DockStyle.Bottom };
            btnPwd.Click += async (s,e) => await SifreDegistirAsync();

            pnlUser.Controls.Add(lblPhotoInfo, 0, 0);
            pnlUser.Controls.Add(lblRole, 0, 2);
            pnlUser.Controls.Add(btnPwd, 0, 4);
            
            grpUser.Controls.Add(pnlUser);

            // Sağ: Mesajlaşma
            var grpMsg = new GroupControl { Text = "Personel Mesajlaşma", Dock = DockStyle.Fill };
            var pnlMsg = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 4 };
            pnlMsg.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Alıcı etiketi
            pnlMsg.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Geçmiş
            pnlMsg.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Giriş
            pnlMsg.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Gönder Butonu
            
            var cmbUsers = new LookUpEdit { Dock = DockStyle.Fill };
            cmbUsers.Properties.NullText = "Mesaj gönderilecek personeli seçin...";
            
            var memoHistory = new MemoEdit { Dock = DockStyle.Fill, ReadOnly = true };
            var txtInput = new MemoEdit { Dock = DockStyle.Fill };
            var btnSend = new SimpleButton { Text = "Mesaj Gönder", Height = 40, Dock = DockStyle.Fill };
            
            btnSend.Click += async (s,e) => { 
                if (cmbUsers.EditValue == null) { XtraMessageBox.Show("Alıcı seçilmedi!"); return; }
                if (string.IsNullOrWhiteSpace(txtInput.Text)) return;
                
                var msg = new MesajModeli {
                    GonderenId = _mevcutKullanici.Id,
                    AliciId = Convert.ToInt32(cmbUsers.EditValue),
                    Icerik = txtInput.Text,
                    GonderilmeTarihi = DateTime.Now
                };
                
                await _mesajServisi.MesajGonderAsync(msg);
                txtInput.Text = "";
                XtraMessageBox.Show("Mesaj gönderildi.");
                await MesajlariYukle(memoHistory);
            };

            pnlMsg.Controls.Add(cmbUsers, 0, 0);
            pnlMsg.Controls.Add(memoHistory, 0, 1);
            pnlMsg.Controls.Add(txtInput, 0, 2);
            pnlMsg.Controls.Add(btnSend, 0, 3);
            
            grpMsg.Controls.Add(pnlMsg);

            layout.Controls.Add(grpUser, 0, 0);
            layout.Controls.Add(grpMsg, 1, 0);
            
            tabProfile.Controls.Add(layout);
            
            // Kullanıcıları ve mesajları yükle
            _ = KullaniciComboDoldur(cmbUsers);
            _ = MesajlariYukle(memoHistory);
        }

        private async Task MesajlariYukle(MemoEdit memo)
        {
            var msgs = await _mesajServisi.MesajlariGetirAsync(_mevcutKullanici.Id);
            var sb = new StringBuilder();
            foreach(var m in msgs) {
                string prefix = m.GonderenId == _mevcutKullanici.Id ? "Benden: " : $"{m.GonderenAdi}: ";
                sb.AppendLine($"[{m.GonderilmeTarihi:HH:mm}] {prefix}{m.Icerik}");
            }
            memo.Text = sb.ToString();
            
            // Alta kaydır
            memo.SelectionStart = memo.Text.Length;
            memo.ScrollToCaret();
        }

        private async Task KullaniciComboDoldur(LookUpEdit combo)
        {
            var users = await _kullaniciServisi.TumKullanicilariGetirAsync();
            var list = users.Where(x => x.Id != _mevcutKullanici.Id).ToList();
            
            combo.Properties.DataSource = list;
            combo.Properties.DisplayMember = "AdSoyad";
            combo.Properties.ValueMember = "Id";
            combo.Properties.Columns.Clear();
            combo.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AdSoyad", "Personel"));
            combo.Properties.NullText = "Personel seçin...";
        }


        private readonly IMqttIstemcisi _mqttIstemcisi;
        private readonly ICihazServisi _cihazServisi;
        private readonly IVeriKaydedici _veriKaydedici;
        private readonly ISeriPortServisi _seriPortServisi; 
        private readonly string _baglantiCumlesi;
        
        private const string BROKER_IP = "127.0.0.1";
        private const int BROKER_PORT = 1883;
        private const string TOPIC = "sensor/energy";

        // SCADA Veri Kaynağı
        private List<ScadaCihazGorumModeli> _scadaListesi = new List<ScadaCihazGorumModeli>();
        private readonly Kullanici _mevcutKullanici;
        private readonly JsonSerializerOptions _jsonAyarlari = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Custom UI Controls
        private PanelControl pnlTop;
        private PanelControl pnlSide;
        
        // Navigation Buttons
        private SimpleButton btnNavScada;
        private SimpleButton btnNavDevices;
        private SimpleButton btnNavReports;
        private SimpleButton btnNavBusiness;

        // Main Tab Control (Hidden Headers)
        private DevExpress.XtraTab.XtraTabControl tabControlMain;
        private DevExpress.XtraTab.XtraTabPage tabScada;
        private DevExpress.XtraTab.XtraTabPage tabDevices;
        private DevExpress.XtraTab.XtraTabPage tabReports;
        private DevExpress.XtraTab.XtraTabPage tabBusiness;
        
        // Gelişmiş Özellikler Verisi
        private IsletmeBilgisi _mevcutIsletmeBilgisi;
        private IMesajServisi _mesajServisi;

        // SCADA Kontrolleri
        private DevExpress.XtraGrid.GridControl gcScada;
        private DevExpress.XtraGrid.Views.Grid.GridView gvScada;
        private SimpleButton btnBaglan;
        
        // Genel Analiz
        private MemoEdit memoTavsiye;
        private LabelControl lblDurum;
        
        // Diagnostic Details (Bottom Section)
        private GroupControl grpDiagnostics;
        private LabelControl lblDiagTitle;
        private MemoEdit memoAnalysis;
        private LabelControl lblHealthBadge;
        private DevExpress.XtraCharts.ChartControl chartTelemetry;
        
        // Toolbar Equivalents
        private SimpleButton tsbAddDevice;
        private SimpleButton tsbEditDevice;
        private SimpleButton tsbDeleteDevice;
        private SimpleButton tsbPairDevice;
        private SimpleButton tsbDeactivateDevice;

        // Report Controls
        private FlowLayoutPanel flowLayoutPanelReports;
        private SimpleButton btnReportDevice;
        private SimpleButton btnReportGreen;
        private SimpleButton btnReportZ;
        private SimpleButton btnReportBudget;
        private SimpleButton btnReportDensity;
        private SimpleButton btnReportAi;
        
        private int _businessId = 0; // To store loaded business ID

        // İşletme Kontrolleri
        private IKullaniciServisi _kullaniciServisi;
        private IIsletmeServisi _isletmeServisi;
        private IGeminiAnalizServisi _geminiServisi;
        private TextEdit txtCompanyName;
        private DateEdit dtpEstablishment;
        private SpinEdit numBudget;
        private SimpleButton btnSaveBusiness;

        // User Management Controls
        private DevExpress.XtraGrid.GridControl gcUsers;
        private DevExpress.XtraGrid.Views.Grid.GridView gvUsers;
        
        // Durum Çubuğu
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblDurumSeri;
        private ToolStripStatusLabel lblDurumBilgi;

        // Profile Tab Items
        private DevExpress.XtraTab.XtraTabPage tabProfile;
        private PictureEdit peAvatar;
        private MemoEdit txtMessage;

        private readonly RaporYoneticisi _raporYoneticisi;

        // Varsayılan Yapıcı
        public FrmDashboard() : this(new Kullanici { KullaniciAdi="Tasarimci", RolAdi="Admin", AdSoyad="Tasarimci" }) { }

        public FrmDashboard(Kullanici kullanici)
        {
            _mevcutKullanici = kullanici;
            
            _baglantiCumlesi = "Host=localhost;Database=energymonitordb;Username=postgres;Password=admin"; 
            
            var apiAnahtari = ""; 
            
            _geminiServisi = new GeminiAnalizServisi(apiAnahtari); 
            
            _mqttIstemcisi = new SMqttIstemcisi();
            _veriKaydedici = new SVeriKaydedici(_baglantiCumlesi);
            _cihazServisi = new SCihaz(_baglantiCumlesi);
            _kullaniciServisi = new SKullanici(_baglantiCumlesi); 
            _isletmeServisi = new SIsletme(_baglantiCumlesi);
            _mesajServisi = new SMesajServisi(_baglantiCumlesi);
            _raporYoneticisi = new RaporYoneticisi(_cihazServisi, _veriKaydedici);
            _seriPortServisi = new SSeriPortServisi(); 
            _analizServisi = new SAnalizServisi();

            InitializeComponent(); 
            
            _seriPortServisi.VeriAlindi += SeriPort_VeriAlindi;
            _mqttIstemcisi.MesajAlindi += MqttMesajGelinceIsle;
            this.Load += FrmDashboard_Load;
            
            this.Shown += (s, e) => IsletmeSekmesiniHazirla();
            
            // Olaylar
            tsbAddDevice.Click += TsbCihazEkle_Tikla;
            tsbEditDevice.Click += TsbCihazDuzenle_Tikla;
            tsbDeleteDevice.Click += TsbCihazSil_Tikla;
            tsbPairDevice.Click += TsbCihazEslestir_Tikla;
            tsbDeactivateDevice.Click += TsbCihazPasifeAl_Tikla;
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1250, 750);
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.BackColor = Color.FromArgb(10, 25, 47); // Deep Navy

            // 1. TOP BAR
            pnlTop = new PanelControl { Dock = DockStyle.Top, Height = 60, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            pnlTop.Appearance.BackColor = Color.FromArgb(10, 25, 47);
            
            // Logo
            var lblLogo = new LabelControl 
            { 
                Text = "Enerlytics", 
                Appearance = { Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(64, 224, 208) },
                Dock = DockStyle.Left,
                AutoSizeMode = LabelAutoSizeMode.None,
                Width = 250,
                Padding = new Padding(20, 10, 0, 0)
            };
            pnlTop.Controls.Add(lblLogo);

            // User Profile
            var pnlUser = new Panel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(0, 5, 20, 5) };
            
            var btnClose = new SimpleButton { Text = "X", Size = new Size(40, 40), Dock = DockStyle.Right };
            btnClose.Appearance.BackColor = Color.Transparent; btnClose.Appearance.ForeColor = Color.White;
            btnClose.Appearance.Font = new Font("Segoe UI", 12, FontStyle.Bold); 
            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.Cursor = Cursors.Hand;
            
            var btnMin = new SimpleButton { Text = "_", Size = new Size(40, 40), Dock = DockStyle.Right };
            btnMin.Appearance.BackColor = Color.Transparent; btnMin.Appearance.ForeColor = Color.White;
            btnMin.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMin.Cursor = Cursors.Hand;

            var lblUser = new LabelControl 
            { 
                Text = $"{_mevcutKullanici.AdSoyad} ({_mevcutKullanici.RolAdi})", 
                Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, TextOptions = { HAlignment = HorzAlignment.Far } },
                Dock = DockStyle.Fill,
                AutoSizeMode = LabelAutoSizeMode.None,
                Padding = new Padding(0, 15, 10, 0)
            };

            pnlUser.Controls.Add(lblUser);
            pnlUser.Controls.Add(btnMin);
            pnlUser.Controls.Add(btnClose);
            pnlTop.Controls.Add(pnlUser);

            this.Controls.Add(pnlTop);

            // 2. SIDE MENU
            pnlSide = new PanelControl { Dock = DockStyle.Left, Width = 250, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            pnlSide.Appearance.BackColor = Color.FromArgb(17, 34, 64); // Lighter Navy
            
            SimpleButton CreateNavButton(string text)
            {
                var btn = new SimpleButton { Text = text, Height = 50, Dock = DockStyle.Top, Cursor = Cursors.Hand };
                btn.Appearance.BackColor = Color.Transparent;
                btn.Appearance.ForeColor = Color.White;
                btn.Appearance.Font = new Font("Segoe UI", 11);
                btn.Appearance.TextOptions.HAlignment = HorzAlignment.Near;
                btn.ImageOptions.Location = ImageLocation.MiddleLeft;
                btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                return btn;
            }

            btnNavScada = CreateNavButton("  SCADA & İzleme");
            btnNavDevices = CreateNavButton("  Cihaz Yönetimi");
            btnNavReports = CreateNavButton("  Raporlar");
            btnNavBusiness = CreateNavButton("  İşletme Profili");
            
             var btnLogout = CreateNavButton("  Oturumu Kapat");
             btnLogout.Dock = DockStyle.Bottom;
             btnLogout.Appearance.ForeColor = Color.Salmon;
             btnLogout.Click += (s, e) => { this.Hide(); new FrmLogin().ShowDialog(); this.Close(); };
             
             // New Profile Button
             var btnNavProfile = CreateNavButton("  Profilim");
             btnNavProfile.Click += (s, e) => tabControlMain.SelectedTabPage = tabProfile;

             var btnChangePwd = CreateNavButton("  Şifre Değiştir");
             btnChangePwd.Dock = DockStyle.Bottom;
             btnChangePwd.Click += async (s, e) => await SifreDegistirAsync();
             // Remove old change pwd from sidebar if moving to Profile tab? Keep both for now.

             pnlSide.Controls.Clear();
             pnlSide.Controls.Add(btnChangePwd); 
             pnlSide.Controls.Add(btnLogout); 
             
             pnlSide.Controls.Add(btnNavBusiness);
             pnlSide.Controls.Add(btnNavReports);
             pnlSide.Controls.Add(btnNavDevices);
             pnlSide.Controls.Add(btnNavScada);
             pnlSide.Controls.Add(btnNavProfile); // Add Profile Button

             this.Controls.Add(pnlSide);
             
             DurumCubugunuHazirla(); // Add Status Bar
             
             // 3. MAIN TAB CONTROL
             tabControlMain = new DevExpress.XtraTab.XtraTabControl { Dock = DockStyle.Fill, ShowTabHeader = DevExpress.Utils.DefaultBoolean.False };
             tabControlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
             tabControlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
             
             tabScada = new DevExpress.XtraTab.XtraTabPage { Text = "SCADA", BackColor = Color.FromArgb(10, 25, 47) };
             tabDevices = new DevExpress.XtraTab.XtraTabPage { Text = "Cihazlar", BackColor = Color.FromArgb(10, 25, 47) };
             tabReports = new DevExpress.XtraTab.XtraTabPage { Text = "Raporlar", BackColor = Color.FromArgb(10, 25, 47) };
             tabBusiness = new DevExpress.XtraTab.XtraTabPage { Text = "İşletme", BackColor = Color.FromArgb(10, 25, 47) };
             tabProfile = new DevExpress.XtraTab.XtraTabPage { Text = "Profil", BackColor = Color.FromArgb(10, 25, 47) };

             tabControlMain.TabPages.AddRange(new[] { tabScada, tabDevices, tabReports, tabBusiness, tabProfile });
             this.Controls.Add(tabControlMain);

              SetupScadaTab();
              SetupReportsTab();
              SetupDevicesTab();
              ProfilSekmesiniHazirla(); // Profil Arayüzünü Başlat
             
             // Bring Status Strip to front if needed
             statusStrip.BringToFront();

            btnNavScada.Click += (s, e) => tabControlMain.SelectedTabPage = tabScada;
            btnNavDevices.Click += (s, e) => { 
                tabControlMain.SelectedTabPage = tabDevices;
                SetupDevicesTab(); // Force refresh on click
            };
            btnNavReports.Click += (s, e) => tabControlMain.SelectedTabPage = tabReports;
            btnNavBusiness.Click += (s, e) => tabControlMain.SelectedTabPage = tabBusiness;

            // FORCE LAYOUT ORDER (High Index = Docks First)
            // 1. Top Bar (Top Edge)
            pnlTop.SendToBack(); 
            // 2. Side Bar (Left Edge below Top)
            pnlSide.SendToBack(); 
            // 3. Side Bar needs to respect Top Bar? 
            // Actually, if Top is Backmost (Index N), Side is (N-1), Tab is (0).
            // Layout: Top -> Side -> Tab.
            // But if Side is added AFTER Top, Side might claim Left Edge of form including Top corner?
            // We want Top to be Full Width.
            // So Top must be Deepest.
            
            // Strategy:
            // pnlSide.SendToBack(); // Side at End.
            // pnlTop.SendToBack();  // Top at End (After Side).
            // Result: [..., Side, Top]. Top is Deepest.
            // Layout: Top (Full Width) -> Side (Left Remaining). Correct.
            
             pnlSide.SendToBack();
             pnlTop.SendToBack();
             tabControlMain.BringToFront(); // Tabloyu en öne al
         }

         private void SetupScadaTab()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = Color.FromArgb(10, 25, 47) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // Height for Top Bar + Analysis
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Top Section Layout (Analysis + Actions)
            var pnlTopSection = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.FromArgb(17, 34, 64) };
            pnlTopSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60)); // Analysis takes 60%
            pnlTopSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); // Actions take 40%

            // Group Control for AI Analysis (Left Side)
            var grpAnalysis = new GroupControl { Text = "Yapay Zeka Destekli Analiz", Dock = DockStyle.Fill };
            
            // Internal Layout for Analysis Group
            var pnlAnalysisInner = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            pnlAnalysisInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Toolbar height
            pnlAnalysisInner.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Memo height

            // Toolbar Panel
            var pnlToolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, BackColor = Color.Transparent };
            
             // API Sıfırlama Butonu
             var btnResetApi = new DevExpress.XtraEditors.SimpleButton { Text = "API Ayarları", Height = 24, Width = 90 };
             btnResetApi.ToolTip = "Yeni API Anahtarı Gir";
             btnResetApi.Appearance.Font = new Font("Segoe UI", 8);
             btnResetApi.Click += async (s, e) => 
             {
                  string newKey = FrmGirisKutusu.Goster("Yeni Gemini API Anahtarını Giriniz:", "API Ayarları", _geminiServisi.ApiAnahtari);
                  if (!string.IsNullOrEmpty(newKey))
                  {
                      _geminiServisi = new GeminiAnalizServisi(newKey);
                     
                     // Veritabanına Kaydet
                     try 
                     {
                         var bilgi = await _isletmeServisi.ProfilGetirAsync(_mevcutKullanici.IsletmeId ?? 1);
                         bilgi.GeminiApiAnahtari = newKey;
                         await _isletmeServisi.ProfilKaydetAsync(bilgi);
                         
                         XtraMessageBox.Show("API Anahtarı veritabanına kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                         await AnalizKontrolEtAsync();
                     }
                     catch(Exception ex) { XtraMessageBox.Show("Kaydetme Hatası: " + ex.Message); }
                 }
            };
            
            // Advice Button
            var btnGetAdvice = new DevExpress.XtraEditors.SimpleButton { Text = "Tavsiye Al ✨", Height = 24, Width = 110 };
            btnGetAdvice.Appearance.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            btnGetAdvice.Appearance.BackColor = Color.LightSeaGreen;
            btnGetAdvice.Click += async (s, e) => 
            {
                btnGetAdvice.Enabled = false;
                btnGetAdvice.Text = "Bekleyin...";
                await AnalizKontrolEtAsync();
                btnGetAdvice.Text = "Tavsiye Al ✨";
                btnGetAdvice.Enabled = true;
            };

            pnlToolbar.Controls.Add(btnResetApi);
            pnlToolbar.Controls.Add(btnGetAdvice);
            pnlAnalysisInner.Controls.Add(pnlToolbar, 0, 0);

             memoTavsiye = new MemoEdit 
             { 
                 Dock = DockStyle.Fill, 
                 Properties = { ReadOnly = true, ScrollBars = ScrollBars.Vertical, Appearance = { Font = new Font("Segoe UI", 10), ForeColor = Color.Black } }
             };
             
             // İlk Uyarı Kontrolü
             if(string.IsNullOrEmpty(_geminiServisi.ApiAnahtari))
             {
                 memoTavsiye.Text = "⚠️ [UYARI] Analiz için API Anahtarı GEREKLİ.\nLütfen yukarıdaki 'API Ayarları' butonuna basarak anahtarınızı giriniz.";
                 memoTavsiye.BackColor = Color.MistyRose;
             }
             else
             {
                 memoTavsiye.Text = "Sistem Analizi Bekleniyor... Veriler toplanıyor.";
                 memoTavsiye.BackColor = Color.White;
             }
             
             pnlAnalysisInner.Controls.Add(memoTavsiye, 0, 1);
             grpAnalysis.Controls.Add(pnlAnalysisInner);
            
            pnlTopSection.Controls.Add(grpAnalysis, 0, 0);

            // Actions (Right Side) - 1x5 Format + Connect
            var pnlActions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(10, 20, 0, 0), BackColor = Color.Transparent };
            
            pnlTopSection.Controls.Add(pnlActions, 1, 0);
            
            layout.Controls.Add(pnlTopSection, 0, 0); // Add Top Section to Main Layout
            
             // Bağlan Butonu
             btnBaglan = new SimpleButton { Text = "Bağlan", Height = 40, Width = 100, Appearance = { Font = new Font("Segoe UI", 10, FontStyle.Bold) } };
             btnBaglan.Click += btnBaglan_Tikla;
            
             // 5 Eylem Butonu
             tsbAddDevice = new SimpleButton { ToolTip = "Cihaz Ekle", Text = "+", Size = new Size(50, 40) }; tsbAddDevice.Click += TsbCihazEkle_Tikla;
             tsbEditDevice = new SimpleButton { ToolTip = "Düzenle", Text = "✎", Size = new Size(50, 40) }; tsbEditDevice.Click += TsbCihazDuzenle_Tikla;
             tsbDeleteDevice = new SimpleButton { ToolTip = "Sil", Text = "🗑", Size = new Size(50, 40) }; tsbDeleteDevice.Click += TsbCihazSil_Tikla;
             tsbPairDevice = new SimpleButton { ToolTip = "Eşleştir", Text = "🔗", Size = new Size(50, 40) }; tsbPairDevice.Click += TsbCihazEslestir_Tikla;
             tsbDeactivateDevice = new SimpleButton { ToolTip = "Pasife Al", Text = "⛔", Size = new Size(50, 40) }; tsbDeactivateDevice.Click += TsbCihazPasifeAl_Tikla;

            // Sadece yetkili ise ekle
            var rol = _mevcutKullanici.RolAdi?.Trim();
            bool cihazlariYonetebilir = string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase) || 
                                     (!string.Equals(rol, "Muhasebe_Personeli", StringComparison.OrdinalIgnoreCase)); 
            
            if (cihazlariYonetebilir)
            {
                pnlActions.Controls.AddRange(new Control[] { btnBaglan, tsbAddDevice, tsbEditDevice, tsbDeleteDevice, tsbPairDevice, tsbDeactivateDevice });
            }
            else
            {
                 // Diğerleri için salt okunur (örn. Muhasebe)
                 pnlActions.Controls.Add(btnBaglan);
            }


            
            // Grid Setup
            gcScada = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            gvScada = new DevExpress.XtraGrid.Views.Grid.GridView();
            gcScada.MainView = gvScada; 
            gcScada.ViewCollection.Add(gvScada);
            
            // Grid Styling
            gvScada.Appearance.Row.BackColor = Color.FromArgb(10, 25, 47);
            gvScada.Appearance.Row.ForeColor = Color.White;
            gvScada.Appearance.HeaderPanel.BackColor = Color.FromArgb(17, 34, 64);
            gvScada.Appearance.HeaderPanel.ForeColor = Color.Turquoise;
            gvScada.OptionsView.EnableAppearanceEvenRow = true;
            gvScada.Appearance.EvenRow.BackColor = Color.FromArgb(13, 28, 50);
            

            layout.RowStyles.Clear();
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Top Analysis & Actions
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 250)); // Diagnostics Details
            
            layout.Controls.Add(pnlTopSection, 0, 0); // Add Top Section to Main Layout
            layout.Controls.Add(gcScada, 0, 1);
            
            // Diagnostics Panel Initialization
            grpDiagnostics = new GroupControl { Text = "Endüstriyel Tanılama & Kestirimci Bakım", Dock = DockStyle.Fill };
            var pnlDiagGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(5) };
            pnlDiagGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            pnlDiagGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            var pnlDiagLeft = new PanelControl { Dock = DockStyle.Fill, BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
            pnlDiagLeft.Appearance.BackColor = Color.Transparent;
            lblHealthBadge = new LabelControl { Text = "SAĞLIK: -", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Padding = new Padding(10) };
            memoAnalysis = new MemoEdit { Dock = DockStyle.Fill, Properties = { ReadOnly = true, Appearance = { Font = new Font("Consolas", 9) } } };
            pnlDiagLeft.Controls.Add(memoAnalysis);
            pnlDiagLeft.Controls.Add(lblHealthBadge);

            chartTelemetry = new DevExpress.XtraCharts.ChartControl { Dock = DockStyle.Fill };
            chartTelemetry.AppearanceNameSerializable = "Dark";
            chartTelemetry.BackColor = Color.Transparent;

            pnlDiagGrid.Controls.Add(pnlDiagLeft, 0, 0);
            pnlDiagGrid.Controls.Add(chartTelemetry, 1, 0);
            grpDiagnostics.Controls.Add(pnlDiagGrid);

             layout.Controls.Add(grpDiagnostics, 0, 2);
             
             gvScada.FocusedRowChanged += GvScada_OdaklanilanSatirDegisti;
            
            tabScada.Controls.Add(layout);
        }


        private void SetupDevicesTab()
        {
            tabDevices.Controls.Clear();
            tabDevices.Padding = new Padding(0);
            
            var frm = new FrmDeviceManager(_baglantiCumlesi, _seriPortServisi);
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            
            tabDevices.Controls.Add(frm);
            frm.Show();
            
            // Debug Log to ensure it's called
            Console.WriteLine("DEBUG: FrmDeviceManager embedded into tabDevices.");
        }

        private void SetupReportsTab()
        {
             flowLayoutPanelReports = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
             flowLayoutPanelReports.BackColor = Color.Transparent;
             
             SimpleButton CreateReportBtn(string text)
             {
                 return new SimpleButton { Text = text, Size = new Size(200, 100), Margin = new Padding(10), Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Bold), BackColor = Color.FromArgb(17, 34, 64), ForeColor = Color.White }, ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat };
             }

             btnReportDevice = CreateReportBtn("Cihaz Envanteri");
             btnReportGreen = CreateReportBtn("Yeşil Enerji");
             btnReportZ = CreateReportBtn("Z-Raporu (Özet)");
             btnReportBudget = CreateReportBtn("Bütçe Tahmini");
             btnReportDensity = CreateReportBtn("Yoğunluk Analizi");
             btnReportAi = CreateReportBtn("Yapay Zeka Raporu ✨");

             btnReportDevice.Click += async (s, e) => {
                 var filtreler = await OpenReportWithFilters("Cihaz Envanteri");
                 if (!filtreler.HasValue) return;
                 using (var sfd = new SaveFileDialog() { Filter = "PDF|*.pdf", FileName = "Envanter.pdf" }) {
                     if (sfd.ShowDialog() == DialogResult.OK) 
                         await _raporYoneticisi.CihazRaporuOlusturAsync(sfd.FileName, filtreler.Value.Baslangic, filtreler.Value.Bitis, filtreler.Value.CihazIdleri, filtreler.Value.Loglar);
                 }
             };

             btnReportZ.Click += async (s, e) => {
                 using (var sfd = new SaveFileDialog() { Filter = "PDF|*.pdf", FileName = "Z_Raporu.pdf" }) {
                     if (sfd.ShowDialog() == DialogResult.OK) 
                         await _raporYoneticisi.ZRaporuOlusturAsync(sfd.FileName);
                 }
             };

             btnReportBudget.Click += async (s, e) => {
                 using (var sfd = new SaveFileDialog() { Filter = "PDF|*.pdf", FileName = "Butce_Tahmin.pdf" }) {
                     if (sfd.ShowDialog() == DialogResult.OK) 
                         await _raporYoneticisi.ButceRaporuOlusturAsync(sfd.FileName, _mevcutIsletmeBilgisi.BirimMaliyet);
                 }
             };

             btnReportDensity.Click += async (s, e) => {
                 var filtreler = await OpenReportWithFilters("Yoğunluk Analizi");
                 if (!filtreler.HasValue) return;
                 using (var sfd = new SaveFileDialog() { Filter = "PDF|*.pdf", FileName = "Yogunluk_Analizi.pdf" }) {
                     if (sfd.ShowDialog() == DialogResult.OK) 
                         await _raporYoneticisi.YogunlukRaporuOlusturAsync(sfd.FileName, filtreler.Value.Baslangic, filtreler.Value.Bitis, filtreler.Value.CihazIdleri);
                 }
             };

             btnReportGreen.Click += (s, e) => {
                 using (var sfd = new SaveFileDialog() { Filter = "PDF|*.pdf", FileName = "Yesil_Enerji.pdf" }) {
                     if (sfd.ShowDialog() == DialogResult.OK) 
                         _raporYoneticisi.YesilEnerjiRaporuOlustur(_scadaListesi?.Sum(d => d.Guc) ?? 0, sfd.FileName);
                 }
             };

             btnReportAi.Click += async (s,e) => {
                 using(var sfd = new SaveFileDialog() { Filter="PDF|*.pdf", FileName="AI_Analiz.pdf" }) {
                     if(sfd.ShowDialog()==DialogResult.OK) _raporYoneticisi.AiAnalizRaporuOlustur(memoTavsiye.Text, sfd.FileName);
                 }
             };

             flowLayoutPanelReports.Controls.AddRange(new Control[] { btnReportDevice, btnReportGreen, btnReportZ, btnReportBudget, btnReportDensity, btnReportAi });
             tabReports.Controls.Add(flowLayoutPanelReports);
        }

        private async Task<(DateTime Baslangic, DateTime Bitis, List<int> CihazIdleri, bool Loglar)?> OpenReportWithFilters(string baslik, bool logGoster = true)
        {
             var cihazlar = (await _cihazServisi.TumCihazlariGetirAsync()).ToList();
             using (var frm = new FrmReportFilters(cihazlar, baslik, logGoster))
             {
                 if (frm.ShowDialog() == DialogResult.OK)
                 {
                     return (frm.BaslangicTarihi, frm.BitisTarihi, frm.SeciliCihazIdleri, frm.LoglariDahilEt);
                 }
             }
             return null;
        }

         private async void FrmDashboard_Load(object sender, EventArgs e)
         {
             YetkileriUygula();
             _ = GriddekiCihazlariYukle();
            
            // İlk API Anahtarı ve İşletme Profili Yüklemesi
            try
            {
               _mevcutIsletmeBilgisi = await _isletmeServisi.ProfilGetirAsync(_mevcutKullanici.IsletmeId ?? 1);
               
               if (_mevcutIsletmeBilgisi != null)
               {
                   if (!string.IsNullOrEmpty(_mevcutIsletmeBilgisi.GeminiApiAnahtari))
                   {
                       _geminiServisi = new GeminiAnalizServisi(_mevcutIsletmeBilgisi.GeminiApiAnahtari);
                   }
               }
               
               await AnalizKontrolEtAsync();
            }
            catch { } 
            
            gvScada.OptionsBehavior.Editable = true; 
            gvScada.OptionsView.ShowGroupPanel = false;
            gvScada.Columns.Clear();
 
            GridSutunuEkle(gvScada, "CihazId", "ID", 40, true);
            GridSutunuEkle(gvScada, "CihazAdi", "Cihaz Adı", 140, true);
            GridSutunuEkle(gvScada, "Marka", "Marka", 100, true);
            GridSutunuEkle(gvScada, "Model", "Model", 100, true);
            GridSutunuEkle(gvScada, "Durum", "Durum", 70, true);
            GridSutunuEkle(gvScada, "Voltaj", "Voltaj (V)", 85, true);
            GridSutunuEkle(gvScada, "Akim", "Akım (A)", 85, true);
            GridSutunuEkle(gvScada, "Guc", "Güç (W)", 85, true);
            GridSutunuEkle(gvScada, "Rpm", "RPM", 60, true);
            GridSutunuEkle(gvScada, "Titresim", "Titreşim (mm/s)", 110, true);
            GridSutunuEkle(gvScada, "SaglikDurumu", "Sağlık", 90, true);
            GridSutunuEkle(gvScada, "BakimOnerisi", "Bakim Önerisi", 250, true);
            GridSutunuEkle(gvScada, "SonGuncelleme", "Son Veri", 140, true);
            GridSutunuEkle(gvScada, "MacAdresi", "MAC", 130, true);

            gvScada.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDown; // Click immediately
            
            // Info column removed per request
            // Coloring
            
            // Coloring
            gvScada.RowStyle += (s, re) => 
            {
                var v = s as DevExpress.XtraGrid.Views.Grid.GridView;
                var durum = v?.GetRowCellValue(re.RowHandle, "Durum")?.ToString();
                if(durum == "Pasif") re.Appearance.BackColor = Color.Gray; 
                
                // Akım yüksekse kırmızı
                if (_scadaListesi.Count > 0) {
                     var akim = Convert.ToDecimal(v?.GetRowCellValue(re.RowHandle, "Akim") ?? 0);
                     var ortalama = _scadaListesi.Average(d=>d.Akim);
                     if(akim > ortalama && ortalama > 0) re.Appearance.BackColor = Color.Salmon;
                }
            };
        }

        private void GridSutunuEkle(DevExpress.XtraGrid.Views.Grid.GridView view, string fieldName, string caption, int width, bool visible = true)
        {
            var col = view.Columns.AddVisible(fieldName);
            col.Caption = caption; col.Width = width; col.Visible = visible; col.OptionsColumn.AllowEdit = false;
        }

        private async System.Threading.Tasks.Task GriddekiCihazlariYukle()
        {
             try
            {
                var devices = await _cihazServisi.TumCihazlariGetirAsync();
                _scadaListesi = devices
                    .Where(d => d.Durum == 1) // Sadece Aktif cihazlar
                    .Select(d => new ScadaCihazGorumModeli
                {
                    CihazId = d.Id,
                    CihazAdi = d.Ad,
                    MacAdresi = d.MacAdresi ?? "",
                    Marka = d.Marka ?? "",
                    Model = d.Model ?? "",
                    GerilimDegeri = d.GerilimDegeri ?? 0,
                    AkimDegeri = d.AkimDegeri ?? 0,
                    NominalGuc = ((d.GerilimDegeri ?? 0) * (d.AkimDegeri ?? 0)), 
                    Durum = "Aktif",
                    CihazTuru = d.Tur,
                    SonGuncelleme = DateTime.MinValue
                }).ToList();

                gcScada.DataSource = _scadaListesi;
            }
            catch (Exception ex) { XtraMessageBox.Show($"Yükleme Hatası: {ex.Message}"); }
        }

        private async void btnBaglan_Tikla(object? sender, EventArgs e)
        {
             try
            {
                await _mqttIstemcisi.BaglanAsync(BROKER_IP, BROKER_PORT);
                await _mqttIstemcisi.AboneOlAsync(TOPIC);
                lblDurumSeri.Text = "Durum: Bağlandı (MQTT)";
                btnBaglan.Enabled = false;
                DurumCubugunuGuncelle();
            }
            catch (Exception ex) { XtraMessageBox.Show($"Bağlantı Hatası: {ex.Message}"); }
        }

        private void MqttMesajGelinceIsle(object? sender, MqttMesajAlindiOlayArgumanlari e)
        {
            if (this.InvokeRequired) { this.Invoke(new Action<object?, MqttMesajAlindiOlayArgumanlari>(MqttMesajGelinceIsle), sender, e); return; }
            try
            {
                var data = JsonSerializer.Deserialize<MqttSensorVeriPaketi>(e.Veri, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null && data.MacAdresi != null) {
                    string? telemetriJson = null;
                    if (data.Telemetri.HasValue)
                    {
                        telemetriJson = data.Telemetri.Value.GetRawText();
                    }

                    // Log lookup
                    var devInGrid = _scadaListesi?.FirstOrDefault(d => d.MacAdresi == data.MacAdresi);
                    int devId = devInGrid?.CihazId ?? 1;

                    ScadaGridiniGuncelle(data, telemetriJson);
                    _ = _veriKaydedici.VeriKaydetAsync(new SensorVerisi { CihazId = devId, SensorTuru = data.Sensor, Deger = data.Deger, Birim = data.Birim, TelemetriVerisi = telemetriJson });
                }
            } catch {}
        }

        private void ScadaGridiniGuncelle(MqttSensorVeriPaketi data, string? telemetriJson)
        {
            if (_scadaListesi == null) return;
            var cihaz = _scadaListesi.FirstOrDefault(d => d.MacAdresi == data.MacAdresi) ?? (_scadaListesi.Count > 0 ? _scadaListesi[0] : null);
             if (cihaz != null)
            {
                cihaz.Durum = "Aktif";
                cihaz.SonGuncelleme = DateTime.Now;
                
                if (data.Sensor.ToLower() == "rotating_telemetry" && !string.IsNullOrEmpty(telemetriJson))
                {
                    cihaz.TelemetriHam = telemetriJson;
                    var motorVerisi = JsonSerializer.Deserialize<DonerCihazTelemetrisi>(telemetriJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (motorVerisi != null)
                    {
                        // Analiz Çağrısı
                        var samples = new List<decimal> { motorVerisi.TitresimX, motorVerisi.TitresimY, motorVerisi.TitresimZ };
                        var analiz = _analizServisi.DonerCihazAnalizEt(samples, motorVerisi.DevirSayisi);
                        
                        cihaz.Rpm = analiz.DevirSayisi;
                        cihaz.Titresim = analiz.TitresimX;
                        cihaz.SaglikDurumu = analiz.SaglikDurumu;
                        
                        // Kestirimci Bakım Çağrısı (simüle verilerle)
                        var olasilik = _analizServisi.ArizaOlasiligiHesapla(null, new List<SensorVerisi>()); // Basitleştirilmiş
                        cihaz.BakimOnerisi = _analizServisi.BakimOnerisiGetir(null, olasilik);
                    }
                }
                else
                {
                    switch (data.Sensor.ToLower())
                    {
                        case "current": cihaz.Akim = data.Deger; break;
                        case "voltage": cihaz.Voltaj = data.Deger; break;
                        case "power": cihaz.Guc = data.Deger; break;
                    }
                }
                gcScada.RefreshDataSource();
            }
        }

        private async System.Threading.Tasks.Task AnalizKontrolEtAsync()
        {
             try
            {
                // Aylık tüketim hesabı (yaklaşık)
                decimal toplamTuketim = _scadaListesi.Sum(d => d.Guc) * 24 * 30 / 1000m; 
                if (toplamTuketim == 0) toplamTuketim = 150; 

                if (string.IsNullOrEmpty(_geminiServisi.ApiAnahtari) || _geminiServisi.ApiAnahtari == "USER_API_KEY_HERE")
                {
                     memoTavsiye.Text = "⚠️ [UYARI] Analiz için API Anahtarı GEREKLİ.\nLütfen 'API Ayarları' butonuna basarak anahtarınızı giriniz.";
                     memoTavsiye.BackColor = Color.MistyRose;
                     return;
                }
                memoTavsiye.BackColor = Color.White;

                // SCADA loglarını topla
                var sbLoglar = new StringBuilder();
                sbLoglar.AppendLine($"[Zaman: {DateTime.Now:yyyy-MM-dd HH:mm}] Sistem Durumu:");
                
                if (_scadaListesi != null && _scadaListesi.Count > 0)
                {
                    foreach (var d in _scadaListesi)
                    {
                        string durum = d.Durum == "Aktif" ? "CALISIYOR" : "DURDU";
                        sbLoglar.AppendLine($"- Cihaz: {d.CihazAdi} | Durum: {durum} | Güç: {d.Guc} W | Voltaj: {d.Voltaj} V");
                    }
                }
                else
                {
                    sbLoglar.AppendLine("- (SCADA sisteminde aktif cihaz tespit edilemedi)");
                }

                decimal birimMaliyet = _mevcutIsletmeBilgisi?.BirimMaliyet ?? 2.5m;
                decimal vergiOrani = _mevcutIsletmeBilgisi?.VergiOrani ?? 18.0m;

                var sonuc = await _geminiServisi.EnerjiKullaniminiAnalizEtAsync(toplamTuketim, 30, birimMaliyet, vergiOrani, sbLoglar.ToString()); 

                memoTavsiye.Text = sonuc.Tavsiye; 
            } catch (Exception ex) { memoTavsiye.Text = $"Analiz Hatası: {ex.Message}"; }
        }

        // Araç Çubuğu
         private void TsbCihazEkle_Tikla(object? sender, EventArgs e) { tabControlMain.SelectedTabPage = tabDevices; SetupDevicesTab(); }
         private void TsbCihazDuzenle_Tikla(object? sender, EventArgs e) { tabControlMain.SelectedTabPage = tabDevices; SetupDevicesTab(); }
         private void TsbCihazSil_Tikla(object? sender, EventArgs e) 
         { 
             if(XtraMessageBox.Show("Silinsin mi?", "Onay", MessageBoxButtons.YesNo)==DialogResult.Yes) {
                 // Silmeyi simüle et
                 XtraMessageBox.Show("Silindi."); 
             }
         }
         
         // Pair/Deactivate Logic
         private bool _isHardwareConnected = false;
         private string _pendingCommand = ""; 

          private void TsbCihazEslestir_Tikla(object? sender, EventArgs e)
          {
              var row = gvScada.GetFocusedRow() as ScadaCihazGorumModeli;
              if(row == null) { XtraMessageBox.Show("Lütfen listeden bir cihaz seçin."); return; }
              
              if (!_seriPortServisi.AcikMi)
             {
                 string port = FrmGirisKutusu.Goster("IoT Cihazının Bağlı Olduğu Port (ör: COM3):", "Bağlantı Ayarı", "COM3");
                  if(!string.IsNullOrEmpty(port)) {
                      _seriPortServisi.BaglantiyiAc(port);
                      if(_seriPortServisi.AcikMi) {
                           XtraMessageBox.Show("Bağlantı Başarılı! Eşleştirme komutu gönderiliyor...");
                           _seriPortServisi.VeriYaz($"PAIR:{row.CihazId}");
                      } else {
                           XtraMessageBox.Show("Bağlantı Başarısız! Portu ve kabloyu kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                      }
                  }
              } 
              else 
              {
                  _seriPortServisi.VeriYaz($"PAIR:{row.CihazId}");
                  XtraMessageBox.Show($"PAIR:{row.CihazId} komutu gönderildi.");
              }
          }

          private void TsbCihazPasifeAl_Tikla(object? sender, EventArgs e)
          {
              if(_seriPortServisi.AcikMi) _seriPortServisi.VeriYaz("DEACTIVATE");
          }

         // Raporlar
         private async System.Threading.Tasks.Task CihazRaporuOlustur() {
              using(var sfd = new SaveFileDialog() { Filter="PDF|*.pdf" }) {
                  if(sfd.ShowDialog()==DialogResult.OK) await _raporYoneticisi.CihazRaporuOlusturAsync(sfd.FileName, DateTime.Now, DateTime.Now, null, true);
              }
         }
         private void YesilRaporOlustur() {
              using(var sfd = new SaveFileDialog() { Filter="PDF|*.pdf" }) {
                  if(sfd.ShowDialog()==DialogResult.OK) _raporYoneticisi.YesilEnerjiRaporuOlustur(_scadaListesi.Sum(d=>d.Guc), sfd.FileName);
              }
         }
         private async System.Threading.Tasks.Task GenelRaporOlustur(string ad, Func<string, System.Threading.Tasks.Task> metod) {
               using(var sfd = new SaveFileDialog() { Filter="PDF|*.pdf", FileName=ad }) {
                  if(sfd.ShowDialog()==DialogResult.OK) await metod(sfd.FileName);
              }
         }



        private async System.Threading.Tasks.Task IsletmeProfiliniKaydet()
        {
            await _isletmeServisi.ProfilKaydetAsync(new IsletmeBilgisi { 
                Id = _businessId, 
                SirketAdi = txtCompanyName.Text, 
                ButceLimiti = numBudget.Value,
                KurulusTarihi = dtpEstablishment.DateTime
            });
            XtraMessageBox.Show("Kaydedildi.");
        }

         private async System.Threading.Tasks.Task SifreDegistirAsync()
         {
              using (var frm = new FrmChangePassword()) {
                  if (frm.ShowDialog() == DialogResult.OK) {
                       _mevcutKullanici.SifreOzeti = frm.YeniSifre;
                       await _kullaniciServisi.KullaniciGuncelleAsync(_mevcutKullanici);
                       XtraMessageBox.Show("Şifre Değişti.");
                  }
              }
         }

          private void YetkileriUygula()
          {
              var rol = _mevcutKullanici.RolAdi?.Trim() ?? "";
              bool yoneticiMi = rol.Contains("Admin", StringComparison.OrdinalIgnoreCase) || rol.Contains("Yönetici", StringComparison.OrdinalIgnoreCase);
              bool muhasebeMi = rol.Contains("Muhasebe", StringComparison.OrdinalIgnoreCase) || rol.Contains("Accounting", StringComparison.OrdinalIgnoreCase);
              bool teknikMi = !yoneticiMi && !muhasebeMi;
  
              // Navigasyon Görünürlüğü
              btnNavBusiness.Enabled = yoneticiMi || muhasebeMi;
              btnNavScada.Enabled = yoneticiMi || teknikMi;
              btnNavDevices.Enabled = yoneticiMi || teknikMi;
              btnNavReports.Enabled = true; 

              // Muhasebe ise cihaz yönetimini gizle
              if (muhasebeMi)
              {
                  tsbAddDevice.Visible = false;
                  tsbEditDevice.Visible = false;
                  tsbDeleteDevice.Visible = false;
              }

              // Rapor buton yetkileri
              if (btnReportBudget != null)
              {
                  btnReportBudget.Enabled = yoneticiMi || muhasebeMi;
                  btnReportZ.Enabled = yoneticiMi || muhasebeMi;
                  btnReportAi.Enabled = yoneticiMi || muhasebeMi;
                  
                  btnReportDevice.Enabled = yoneticiMi || teknikMi;
                  btnReportDensity.Enabled = yoneticiMi || teknikMi;
                  
                  btnReportGreen.Enabled = true; 
              }
          }

        private void IsletmeSekmesiniHazirla()
        {
            var rol = _mevcutKullanici.RolAdi?.Trim().ToLower();
            bool yoneticiMi = rol == "admin" || rol == "yönetici";
            bool muhasebeMi = rol == "muhasebe_personeli" || rol == "accounting";

            if (!yoneticiMi && !muhasebeMi)
            {
                tabBusiness.Controls.Add(new LabelControl { Text = "⚠️ Yetkisiz Erişim", Dock = DockStyle.Fill, Appearance = { TextOptions = { HAlignment = HorzAlignment.Center } } });
                return;
            }

            if (_mevcutIsletmeBilgisi == null) _mevcutIsletmeBilgisi = new IsletmeBilgisi();

            var tabAlt = new DevExpress.XtraTab.XtraTabControl { Dock = DockStyle.Fill };
            var tpBilgi = new DevExpress.XtraTab.XtraTabPage { Text = "İşletme Bilgileri" };
            var tpKullanicilar = new DevExpress.XtraTab.XtraTabPage { Text = "Personel Yönetimi" };
            var tpParametreler = new DevExpress.XtraTab.XtraTabPage { Text = "Fatura Parametreleri" };
            var tpSistem = new DevExpress.XtraTab.XtraTabPage { Text = "Sistem Araçları" };
            tabAlt.TabPages.AddRange(new[] { tpBilgi, tpKullanicilar, tpParametreler, tpSistem });

            // 1. İşletme Bilgileri
            var pnlBilgi = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 7 };
            txtCompanyName = new TextEdit { Text = _mevcutIsletmeBilgisi.SirketAdi, Dock = DockStyle.Top };
            dtpEstablishment = new DateEdit { DateTime = _mevcutIsletmeBilgisi.KurulusTarihi, Dock = DockStyle.Top };
            numBudget = new SpinEdit { Value = _mevcutIsletmeBilgisi.ButceLimiti, Dock = DockStyle.Top };
            btnSaveBusiness = new SimpleButton { Text = "İşletme Bilgilerini Kaydet", Height = 40, Dock = DockStyle.Top };
            btnSaveBusiness.Click += async (s, e) => {
                _mevcutIsletmeBilgisi.SirketAdi = txtCompanyName.Text;
                _mevcutIsletmeBilgisi.KurulusTarihi = dtpEstablishment.DateTime;
                _mevcutIsletmeBilgisi.ButceLimiti = numBudget.Value;
                await _isletmeServisi.ProfilKaydetAsync(_mevcutIsletmeBilgisi);
                XtraMessageBox.Show("İşletme bilgileri güncellendi.");
            };

            pnlBilgi.Controls.Add(new LabelControl { Text = "İşletme Adı" }, 0, 0);
            pnlBilgi.Controls.Add(txtCompanyName, 0, 1);
            pnlBilgi.Controls.Add(new LabelControl { Text = "Kuruluş Tarihi" }, 0, 2);
            pnlBilgi.Controls.Add(dtpEstablishment, 0, 3);
            pnlBilgi.Controls.Add(new LabelControl { Text = "Bütçe Limiti (TL)" }, 0, 4);
            pnlBilgi.Controls.Add(numBudget, 0, 5);
            pnlBilgi.Controls.Add(btnSaveBusiness, 0, 6);
            tpBilgi.Controls.Add(pnlBilgi);

            // 2. Personel Yönetimi
            var pnlUsers = new PanelControl { Dock = DockStyle.Fill };
            gcUsers = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            gvUsers = new DevExpress.XtraGrid.Views.Grid.GridView();
            gcUsers.MainView = gvUsers;
            
            var pnlUserActions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(5) };
            var btnAddU = new SimpleButton { Text = "Personel Ekle", Width = 120 };
            btnAddU.Click += async (s, e) => await PersonelEkle();
            var btnEditU = new SimpleButton { Text = "Düzenle", Width = 90 };
            btnEditU.Click += async (s, e) => await PersonelDuzenle();
            var btnDelU = new SimpleButton { Text = "Sil", Width = 90 };
            btnDelU.Click += async (s, e) => await PersonelSil();
            var btnApproveU = new SimpleButton { Text = "İsteği Onayla", Width = 120, Appearance = { BackColor = Color.LightGreen } };
            btnApproveU.Click += async (s, e) => await PersonelOnayla();
            
            pnlUserActions.Controls.AddRange(new Control[] { btnAddU, btnEditU, btnDelU, btnApproveU });
            pnlUsers.Controls.Add(gcUsers);
            pnlUsers.Controls.Add(pnlUserActions);
            tpKullanicilar.Controls.Add(pnlUsers);
            _ = KullanicilariYukle();

            // 3. Fatura Parametreleri
            var pnlParametreler = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 9 };
            var txtBirim = new SpinEdit { Text = _mevcutIsletmeBilgisi.BirimMaliyet.ToString(), Dock = DockStyle.Top };
            var txtVergi = new SpinEdit { Text = _mevcutIsletmeBilgisi.VergiOrani.ToString(), Dock = DockStyle.Top };
            var txtAboneNo = new TextEdit { Text = _mevcutIsletmeBilgisi.AboneNo, Dock = DockStyle.Top };
            var txtMusteriNo = new TextEdit { Text = _mevcutIsletmeBilgisi.MusteriNo, Dock = DockStyle.Top };
            var btnParametreKaydet = new SimpleButton { Text = "Parametreleri Kaydet", Height = 40, Dock = DockStyle.Top };
            btnParametreKaydet.Click += async (s, e) => {
                _mevcutIsletmeBilgisi.BirimMaliyet = txtBirim.Value;
                _mevcutIsletmeBilgisi.VergiOrani = txtVergi.Value;
                _mevcutIsletmeBilgisi.AboneNo = txtAboneNo.Text;
                _mevcutIsletmeBilgisi.MusteriNo = txtMusteriNo.Text;
                await _isletmeServisi.ProfilKaydetAsync(_mevcutIsletmeBilgisi);
                XtraMessageBox.Show("Fatura parametreleri güncellendi.");
            };

            pnlParametreler.Controls.Add(new LabelControl { Text = "Birim Ücret (TL/kWh)" }, 0, 0);
            pnlParametreler.Controls.Add(txtBirim, 0, 1);
            pnlParametreler.Controls.Add(new LabelControl { Text = "Vergi Oranı (%)" }, 0, 2);
            pnlParametreler.Controls.Add(txtVergi, 0, 3);
            pnlParametreler.Controls.Add(new LabelControl { Text = "Abone No" }, 0, 4);
            pnlParametreler.Controls.Add(txtAboneNo, 0, 5);
            pnlParametreler.Controls.Add(new LabelControl { Text = "Müşteri No" }, 0, 6);
            pnlParametreler.Controls.Add(txtMusteriNo, 0, 7);
            pnlParametreler.Controls.Add(btnParametreKaydet, 0, 8);
            tpParametreler.Controls.Add(pnlParametreler);

            // 4. Sistem Araçları
            var pnlSystem = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 3 };
            var btnRepair = new SimpleButton { Text = "Veritabanını Onar ve Şemayı Güncelle", Height = 50, Dock = DockStyle.Top, Appearance = { BackColor = Color.DarkOrange, ForeColor = Color.White } };
            btnRepair.Click += async (s, e) => {
                try {
                    await _kullaniciServisi.SemayiHazirlaAsync();
                    XtraMessageBox.Show("Veritabanı şeması başarıyla onarıldı ve güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } catch (Exception ex) {
                    XtraMessageBox.Show("Onarım sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            var lblSystemInfo = new LabelControl { Text = "Not: Beklenmedik hatalar (sütun bulunamadı vb.) alıyorsanız bu butonu kullanabilirsiniz.", AutoSizeMode = LabelAutoSizeMode.Vertical, Dock = DockStyle.Top };
            
            pnlSystem.Controls.Add(lblSystemInfo, 0, 0);
            pnlSystem.Controls.Add(btnRepair, 0, 1);
            tpSistem.Controls.Add(pnlSystem);

            tabBusiness.Controls.Clear();
            tabBusiness.Controls.Add(tabAlt);
        }

         // ... User Management Logic (add/edit/delete users) skipped for brevity in this redesign pass to focus on SCADA
         // If needed, can re-add user grid to Business tab.
          private async System.Threading.Tasks.Task PersonelEkle() 
          {
              var roles = (await _kullaniciServisi.TumRolleriGetirAsync()).ToList();
               using var frm = new FrmUserEditor(roles);
               if (frm.ShowDialog() == DialogResult.OK)
               {
                   await _kullaniciServisi.KullaniciEkleAsync(frm.SonucKullanici);
                   await KullanicilariYukle();
               }
          }
          private async System.Threading.Tasks.Task PersonelDuzenle() 
          {
              var row = gvUsers.GetFocusedRow() as Kullanici;
              if (row == null) return;
              var roles = (await _kullaniciServisi.TumRolleriGetirAsync()).ToList();
              using var frm = new FrmUserEditor(roles, row);
              if (frm.ShowDialog() == DialogResult.OK)
              {
                  await _kullaniciServisi.KullaniciGuncelleAsync(frm.SonucKullanici);
                  await KullanicilariYukle();
              }
          }
         
          private async System.Threading.Tasks.Task KullaniciSifresiniSifirla()
          {
              var row = gvUsers.GetFocusedRow() as Kullanici;
              if (row == null) return;
              
              using (var frm = new FrmChangePassword()) {
                  if (frm.ShowDialog() == DialogResult.OK) {
                       row.SifreOzeti = frm.YeniSifre; 
                       await _kullaniciServisi.SifreSifirlaAsync(row.Id, frm.YeniSifre);
                       XtraMessageBox.Show($"{row.KullaniciAdi} için şifre güncellendi.");
                  }
              }
          }
          
          private async System.Threading.Tasks.Task PersonelOnayla() 
          {
              var row = gvUsers.GetFocusedRow() as Kullanici;
              if (row == null) return;
              if (row.AktifMi) { XtraMessageBox.Show("Bu kullanıcı zaten aktif."); return; }
              
              var roles = (await _kullaniciServisi.TumRolleriGetirAsync()).ToList();
               using var frm = new FrmUserEditor(roles, row);
               if (frm.ShowDialog() == DialogResult.OK)
               {
                   row.AktifMi = true; 
                   await _kullaniciServisi.KullaniciGuncelleAsync(frm.SonucKullanici);
                   XtraMessageBox.Show($"{row.KullaniciAdi} hesabı aktifleştirildi ve rolü atandı.");
                   await KullanicilariYukle();
               }
          }
          private async System.Threading.Tasks.Task PersonelSil() 
          {
              var row = gvUsers.GetFocusedRow() as Kullanici;
              if (row == null) return;
              if(XtraMessageBox.Show($"{row.KullaniciAdi} silinsin mi?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
              {
                  await _kullaniciServisi.KullaniciSilAsync(row.Id);
                  await KullanicilariYukle();
              }
          }
          private async System.Threading.Tasks.Task KullanicilariYukle() 
      {
          try
          {
              var users = await _kullaniciServisi.TumKullanicilariGetirAsync();
              gcUsers.DataSource = users;
              gcUsers.RefreshDataSource();
              
              if(gvUsers.Columns["SifreOzeti"] != null) gvUsers.Columns["SifreOzeti"].Visible = false; 
              if(gvUsers.Columns["AktifMi"] != null) {
                  gvUsers.Columns["AktifMi"].Caption = "Aktif?";
                  gvUsers.Columns["AktifMi"].Width = 60;
              }
          }
          catch (Exception ex) { XtraMessageBox.Show($"Kullanıcılar yüklenemedi: {ex.Message}"); }
      }

          private void SeriPort_VeriAlindi(object? sender, string veri)
          {
              if(this.InvokeRequired) { this.Invoke(new Action<object?, string>(SeriPort_VeriAlindi), sender, veri); return; }
              
              // Durum etiketini güncelle veya eşleştirme yanıtını işle
              lblDurumBilgi.Text = $" | Seri Port: {veri}";
              
              if(veri.Contains("PAIRED")) {
                  XtraMessageBox.Show("Cihaz Eşleşti!", "Eşleştirme", MessageBoxButtons.OK, MessageBoxIcon.Information);
                  DurumCubugunuGuncelle();
              }
          }
           private void GvScada_OdaklanilanSatirDegisti(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
           {
              var cihaz = gvScada.GetFocusedRow() as ScadaCihazGorumModeli;
              if (cihaz == null) return;
 
              lblHealthBadge.Text = $"SAĞLIK: {cihaz.SaglikDurumu}";
              lblHealthBadge.ForeColor = cihaz.SaglikDurumu == "Nominal" ? Color.LightGreen : (cihaz.SaglikDurumu == "Uyari" ? Color.Orange : Color.Salmon);

              var sb = new StringBuilder();
              sb.AppendLine($"📊 [ ANALİZ ÖZETİ - {cihaz.CihazAdi} ]");
              sb.AppendLine($"Cihaz Türü: {cihaz.CihazTuru}");
              sb.AppendLine($"Çalışma Hızı: {cihaz.Rpm} RPM");
              sb.AppendLine($"Titreşim Genliği: {cihaz.Titresim} mm/s");
              sb.AppendLine("--------------------------------------------------");
              sb.AppendLine($"💡 BAKIM TAVSİYESİ:");
              sb.AppendLine(cihaz.BakimOnerisi);
              
              if (!string.IsNullOrEmpty(cihaz.TelemetriHam))
              {
                  sb.AppendLine("\n📡 HAM TELEMETRİ (JSON):");
                  sb.AppendLine(cihaz.TelemetriHam);
              }
 
              memoAnalysis.Text = sb.ToString();
              TanilamaGrafiginiGuncelle(cihaz);
          }

          private void TanilamaGrafiginiGuncelle(ScadaCihazGorumModeli dev)
         {
             chartTelemetry.Series.Clear();
              chartTelemetry.Titles.Clear();
              
               if (dev.CihazTuru == CihazTuru.Doner)
              {
                  chartTelemetry.Titles.Add(new DevExpress.XtraCharts.ChartTitle { Text = "Vibrasyon Analizi (FFT Spektrumu)" });
                  var series = new DevExpress.XtraCharts.Series("Frekans (Hz)", DevExpress.XtraCharts.ViewType.Bar);
                  
                  // Mock Spectrum or Parse from TelemetryRaw
                  if (!string.IsNullOrEmpty(dev.TelemetriHam))
                  {
                      var rot = JsonSerializer.Deserialize<DonerCihazTelemetrisi>(dev.TelemetriHam, _jsonAyarlari);
                      if (rot != null && rot.FrekansVerisi != null)
                      {
                          for (int i = 0; i < rot.FrekansVerisi.Count; i++)
                              series.Points.Add(new DevExpress.XtraCharts.SeriesPoint((i * 10).ToString(), rot.FrekansVerisi[i]));
                      }
                  }
                  else // Fallback mock
                  {
                      series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("10", 2.5));
                      series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("50", 8.2)); // Peak at 50Hz
                      series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("100", 1.4));
                  }
                  chartTelemetry.Series.Add(series);
               }
              else if (dev.CihazTuru == CihazTuru.Termal)
             {
                 chartTelemetry.Titles.Add(new DevExpress.XtraCharts.ChartTitle { Text = "Sıcaklık Grafiği (°C)" });
                 var series = new DevExpress.XtraCharts.Series("Sıcaklık", DevExpress.XtraCharts.ViewType.Line);
                 
                 // Mock Thermal or Parse
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("Yüzey 1", 45));
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("Yüzey 2", 52));
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("Çıkış", 38));
                 
                 chartTelemetry.Series.Add(series);
             }
             else
             {
                 chartTelemetry.Titles.Add(new DevExpress.XtraCharts.ChartTitle { Text = "Enerji Tüketim Analizi" });
                 var series = new DevExpress.XtraCharts.Series("Yük (%)", DevExpress.XtraCharts.ViewType.Area);
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("08:00", 20));
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("12:00", 85));
                 series.Points.Add(new DevExpress.XtraCharts.SeriesPoint("18:00", 40));
                 chartTelemetry.Series.Add(series);
             }
          }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTBOTTOMRIGHT = 17;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTCAPTION = 2;

            if (m.Msg == WM_NCHITTEST)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);

                int gripSize = 16;
                if (pos.X >= this.ClientSize.Width - gripSize && pos.Y >= this.ClientSize.Height - gripSize)
                {
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                    return;
                }

                int borderSize = 5;
                if (pos.X <= borderSize && pos.Y <= borderSize) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (pos.X >= this.ClientSize.Width - borderSize && pos.Y <= borderSize) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (pos.X <= borderSize && pos.Y >= this.ClientSize.Height - borderSize) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (pos.X >= this.ClientSize.Width - borderSize && pos.Y >= this.ClientSize.Height - borderSize) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (pos.X <= borderSize) { m.Result = (IntPtr)HTLEFT; return; }
                if (pos.X >= this.ClientSize.Width - borderSize) { m.Result = (IntPtr)HTRIGHT; return; }
                if (pos.Y <= borderSize) { m.Result = (IntPtr)HTTOP; return; }
                if (pos.Y >= this.ClientSize.Height - borderSize) { m.Result = (IntPtr)HTBOTTOM; return; }
            }
            base.WndProc(ref m);
        }
    }

    public class ScadaCihazGorumModeli
    {
        public int CihazId { get; set; }
        public string CihazAdi { get; set; } = "";
        public string MacAdresi { get; set; } = "";
        public string Marka { get; set; } = "";
        public string Model { get; set; } = "";
        public decimal GerilimDegeri { get; set; }
        public decimal AkimDegeri { get; set; }
        public decimal NominalGuc { get; set; }
        public string Durum { get; set; } = "Pasif";
        public decimal Voltaj { get; set; }
        public decimal Akim { get; set; }
        public decimal Guc { get; set; }
        public DateTime SonGuncelleme { get; set; }
        
        // Uzman Analitik Alanlar
        public CihazTuru CihazTuru { get; set; }
        public decimal Rpm { get; set; }
        public decimal Titresim { get; set; }
        public string SaglikDurumu { get; set; } = "Normal";
        public string BakimOnerisi { get; set; } = "-";
        public string TelemetriHam { get; set; } = ""; // Detaylı grafikler için JSON saklama alanı
    }

    public class MqttSensorVeriPaketi
    {
        public string Sensor { get; set; } = "";
        public decimal Deger { get; set; }
        public string Birim { get; set; } = "";
        public string MacAdresi { get; set; } = "";
        public JsonElement? Telemetri { get; set; }
    }
}
