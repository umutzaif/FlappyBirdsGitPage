using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.SP;
using System.Linq;

namespace EnergyMonitor.Forms
{
    public partial class FrmLogin : XtraForm 
    {
        private readonly SpKullanici _spKullanici;
        public Kullanici? GirisYapanKullanici { get; private set; }

        // UI Kontrolleri
        private TextEdit txtKullaniciAdi;
        private TextEdit txtSifre;
        private CheckEdit chkBeniHatirla;
        private HyperlinkLabelControl lblSifremiUnuttum;
        private SimpleButton btnGiris;
        private LabelControl lblBaslik;
        private PanelControl pnlKart;

        public FrmLogin()
        {
            InitializeComponent();
            _spKullanici = new SpKullanici("Host=localhost;Port=5432;Database=energymonitordb;Username=postgres;Password=admin");
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1280, 800); // Larger default
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            
            // Background Image Logic
            string bgPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "login_bg.png");
            if (System.IO.File.Exists(bgPath))
            {
                this.BackgroundImage = Image.FromFile(bgPath);
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                this.BackColor = Color.FromArgb(10, 25, 47); 
            }

            // 1. Top Bar
            // Height increased for better spacing
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.Transparent, Padding = new Padding(30, 0, 30, 0) };
            
            // Logo
            var lblLogo = new LabelControl 
            { 
                Text = "Enerlytics", 
                Appearance = { Font = new Font("Segoe UI", 24, FontStyle.Bold), ForeColor = Color.FromArgb(64, 224, 208) },
                Dock = DockStyle.Left,
                AutoSizeMode = LabelAutoSizeMode.None, 
                Width = 250,
                Padding = new Padding(0, 20, 0, 0) 
            };
            
            // Menü
            var pnlMenu = new FlowLayoutPanel 
            { 
                Dock = DockStyle.Right, 
                FlowDirection = FlowDirection.LeftToRight, 
                AutoSize = true, 
                WrapContents = false, 
                Padding = new Padding(0, 30, 0, 0) 
            };

            string[] menuler = { "IoT", "SCADA", "Verimlilik", "ENERLYTICS" };
            foreach(var m in menuler)
            {
                var lbl = new LabelControl 
                { 
                    Text = m, 
                    Appearance = { Font = new Font("Segoe UI", 11, FontStyle.Regular), ForeColor = Color.White },
                    Margin = new Padding(25, 0, 0, 0),
                    Cursor = Cursors.Hand
                };
                if(m == "ENERLYTICS") lbl.Appearance.Font = new Font("Segoe UI", 11, FontStyle.Bold);
                pnlMenu.Controls.Add(lbl);
            }
            
            // Close Button - FIXED: More Visible
            var btnClose = new SimpleButton { Text = "X", Size = new Size(40, 40) };
            btnClose.Appearance.BackColor = Color.Transparent;
            btnClose.Appearance.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.Cursor = Cursors.Hand;
            // Hack to place close button absolute top-right since FlowLayout is Dock.Right
            // We can add it to pnlMenu for alignment or keep separate. 
            // Design shows it's part of the nav line usually. Let's add to menu for consistent alignment.
            btnClose.Margin = new Padding(20, -5, 0, 0); // Tweaking position
            pnlMenu.Controls.Add(btnClose);
            
            pnlTop.Controls.Add(pnlMenu); 
            pnlTop.Controls.Add(lblLogo);
            
            this.Controls.Add(pnlTop);

            // 2. Merkezi Giriş Kartı
            pnlKart = new PanelControl 
            { 
                Size = new Size(500, 600), 
                LookAndFeel = { Style = DevExpress.LookAndFeel.LookAndFeelStyle.Flat, UseDefaultLookAndFeel = false },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder 
            };
            pnlKart.Appearance.BackColor = Color.FromArgb(50, 0, 0, 0); // %20 karanlık katman
            pnlKart.Location = new Point((this.Width - pnlKart.Width) / 2, (this.Height - pnlKart.Height) / 2);
            
            // Başlık Etiketi (Dinamik)
            lblHeader = new LabelControl 
            { 
                Text = "Enerji İzleme Sistemine Hoşgeldiniz", 
                Appearance = { Font = new Font("Segoe UI Light", 22, FontStyle.Regular), ForeColor = Color.Cyan },
                AutoSize = true
            };
            this.Controls.Add(lblHeader);
            lblHeader.Location = new Point((this.Width - lblHeader.Width) / 2, pnlKart.Top - 60);

            // Başlık
            lblBaslik = new LabelControl 
            { 
                Text = "Giriş Yap", 
                Appearance = { Font = new Font("Segoe UI", 32, FontStyle.Regular), ForeColor = Color.White, TextOptions = { HAlignment = HorzAlignment.Center } },
                AutoSizeMode = LabelAutoSizeMode.None,
                Size = new Size(500, 60)
            };
            pnlKart.Controls.Add(lblBaslik); 
            lblBaslik.Top = 60; 
            lblBaslik.Left = 0;

            // Kullanıcı Adı
            txtKullaniciAdi = new TextEdit 
            { 
                Properties = { NullValuePrompt = "Kullanıcı Adı", NullValuePromptShowForEmptyValue = true },
                Size = new Size(400, 45)
            };
            txtKullaniciAdi.Properties.Appearance.BackColor = Color.FromArgb(23, 42, 70); 
            txtKullaniciAdi.Properties.Appearance.ForeColor = Color.White;
            txtKullaniciAdi.Properties.Appearance.Font = new Font("Segoe UI", 12);
            txtKullaniciAdi.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder; 
            var divUser = new Panel { Size = new Size(400, 1), BackColor = Color.Gray, Top = 205, Left = 50 };
            pnlKart.Controls.Add(divUser);
            
            pnlKart.Controls.Add(txtKullaniciAdi); 
            txtKullaniciAdi.Top = 160; 
            txtKullaniciAdi.Left = 50;

            // Şifre
            txtSifre = new TextEdit 
            { 
                Properties = { NullValuePrompt = "Şifre", NullValuePromptShowForEmptyValue = true, UseSystemPasswordChar = true }, 
                Size = new Size(400, 45)
            };
            txtSifre.Properties.Appearance.BackColor = Color.FromArgb(23, 42, 70); 
            txtSifre.Properties.Appearance.ForeColor = Color.White;
            txtSifre.Properties.Appearance.Font = new Font("Segoe UI", 12);
            txtSifre.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            var divPass = new Panel { Size = new Size(400, 1), BackColor = Color.Gray, Top = 285, Left = 50 };
            pnlKart.Controls.Add(divPass);

            pnlKart.Controls.Add(txtSifre); 
            txtSifre.Top = 240; 
            txtSifre.Left = 50;

            // Bağlantılar (Beni Hatırla / Şifremi Unuttum)
            chkBeniHatirla = new CheckEdit { Text = "Beni Hatırla", ForeColor = Color.LightGray };
            chkBeniHatirla.Properties.Appearance.Font = new Font("Segoe UI", 9);
            chkBeniHatirla.Properties.Appearance.ForeColor = Color.LightGray;
            chkBeniHatirla.Top = 310; chkBeniHatirla.Left = 50;
            pnlKart.Controls.Add(chkBeniHatirla);

            lblSifremiUnuttum = new HyperlinkLabelControl { Text = "Şifremi Unuttum?", ForeColor = Color.Gray };
            lblSifremiUnuttum.Appearance.LinkColor = Color.Gray;
            lblSifremiUnuttum.Top = 313; lblSifremiUnuttum.Left = 340;
            lblSifremiUnuttum.Click += (s, e) => XtraMessageBox.Show("Sistem yöneticiniz (Admin) ile iletişime geçerek şifre sıfırlama talebinde bulunabilirsiniz.", "Şifre Sıfırlama");
            pnlKart.Controls.Add(lblSifremiUnuttum);

            // Buton
            btnGiris = new SimpleButton 
            { 
                Text = "Giriş Yap", 
                Height = 55, 
                Width = 400,
                Appearance = { BackColor = Color.White, ForeColor = Color.Black, Font = new Font("Segoe UI", 14, FontStyle.Bold) }
            };
            btnGiris.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            btnGiris.Click += btnGiris_Click;
            btnGiris.Cursor = Cursors.Hand;
            pnlKart.Controls.Add(btnGiris); 
            btnGiris.Top = 380; 
            btnGiris.Left = 50;

            // Veya Kayıt Ol
            var lblVeya = new HyperlinkLabelControl 
            { 
                Text = "veya Kayıt ol", 
                ForeColor = Color.Gray, 
                AutoSize = true,
                Appearance = { Font = new Font("Segoe UI", 10), LinkColor = Color.LightBlue }
            };
            lblVeya.Click += async (s, e) => await KayitYonet();
            pnlKart.Controls.Add(lblVeya); 
            lblVeya.Top = 460; 
            lblVeya.Left = (pnlKart.Width - 60)/2; 

            this.Controls.Add(pnlKart);
            
            this.Resize += (s,e) => 
            {
                pnlKart.Location = new Point((this.Width - pnlKart.Width) / 2, (this.Height - pnlKart.Height) / 2);
            };
            
            this.Load += FrmLogin_Load;
        }

        private LabelControl lblHeader;

        private async void FrmLogin_Load(object sender, EventArgs e)
        {
             btnGiris.Enabled = false;
             try 
             {
                  // 1. Sessiz Migrasyon
                  await _spKullanici.SemayiHazirlaAsync();
                  
                  // 2. Basit Branding
                  var isletmeServisi = new EnergyMonitor.Service.SIsletme("Host=localhost;Database=energymonitordb;Username=postgres;Password=admin");
                  var bilgi = await isletmeServisi.ProfilGetirAsync(1);
                  
                  string isletmeAdi = (bilgi != null && !string.IsNullOrEmpty(bilgi.SirketAdi)) ? bilgi.SirketAdi : "Enerlytics";
                  lblHeader.Text = $"{isletmeAdi} Enerji İzleme Giriş Paneli";

                  lblHeader.BringToFront();
                  lblHeader.Location = new Point((this.Width - lblHeader.Width) / 2, pnlKart.Top - 60);
                  
                  btnGiris.Enabled = true;

                  // 3. Beni Hatırla Mantığı
                  string kaydedilmisKullanici = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Enerlytics", "remember.cfg");
                  if (System.IO.File.Exists(kaydedilmisKullanici))
                  {
                      txtKullaniciAdi.Text = System.IO.File.ReadAllText(kaydedilmisKullanici);
                      chkBeniHatirla.Checked = true;
                      txtSifre.Focus();
                  }
              }
              catch(Exception ex)
              {
                  XtraMessageBox.Show("Başlatma Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
        }

        private async System.Threading.Tasks.Task KayitYonet()
        {
            try
            {
                var isletmeler = (await _spKullanici.IsletmeleriGetirAsync()).ToList();
                using (var frm = new FrmSignUp(isletmeler))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        var yeniKullanici = frm.SonucKullanici;
                        await _spKullanici.KullaniciEkleAsync(yeniKullanici);
                        XtraMessageBox.Show("Hesap oluşturma isteğiniz gönderildi. Admin onayından sonra giriş yapabilirsiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGiris_Click(object? sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtKullaniciAdi.Text) || string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                XtraMessageBox.Show("Lütfen kullanıcı adı ve şifre giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try 
            {
                var kullanici = await _spKullanici.GirisYapAsync(txtKullaniciAdi.Text.Trim(), txtSifre.Text.Trim());
                if (kullanici != null)
                {
                    GirisYapanKullanici = kullanici;
                    
                    // Beni Hatırla durumunu kaydet
                    string klasor = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Enerlytics");
                    if (!System.IO.Directory.Exists(klasor)) System.IO.Directory.CreateDirectory(klasor);
                    
                    string kaydedilmisKullaniciYolu = System.IO.Path.Combine(klasor, "remember.cfg");
                    if (chkBeniHatirla.Checked)
                    {
                        System.IO.File.WriteAllText(kaydedilmisKullaniciYolu, txtKullaniciAdi.Text);
                    }
                    else if (System.IO.File.Exists(kaydedilmisKullaniciYolu))
                    {
                        System.IO.File.Delete(kaydedilmisKullaniciYolu);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show("Geçersiz kimlik bilgileri.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch(Exception ex)
            {
                XtraMessageBox.Show("Giriş Hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
