using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Forms
{
    public partial class FrmUserEditor : XtraForm
    {
        public Kullanici SonucKullanici { get; private set; }
        private readonly List<Rol> _roller;
        private readonly bool _duzenlemeModu;

        private TextEdit txtKullaniciAdi;
        private TextEdit txtSifre;
        private TextEdit txtAdSoyad;
        private System.Windows.Forms.ComboBox cmbRol;
        private SimpleButton btnKaydet;
        private SimpleButton btnIptal;

        public FrmUserEditor(List<Rol> roller, Kullanici? mevcutKullanici = null)
        {
            _roller = roller;
            _duzenlemeModu = mevcutKullanici != null;
            SonucKullanici = mevcutKullanici ?? new Kullanici();
            InitializeComponent();
            
            RolleriYukle();
            if (_duzenlemeModu)
            {
                AlanlariDoldur();
                this.Text = "Kullanıcı Düzenle";
                txtKullaniciAdi.Enabled = false; // Kullanıcı adı değiştirilemez
            }
            else
            {
                this.Text = "Yeni Kullanıcı Ekle";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;

            var layout = new System.Windows.Forms.TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.RowCount = 6;
            layout.ColumnCount = 2;
            
            // Kullanıcı Adı
            layout.Controls.Add(new Label { Text = "Kullanıcı Adı:", AutoSize = true }, 0, 0);
            txtKullaniciAdi = new TextEdit { Width = 200 };
            layout.Controls.Add(txtKullaniciAdi, 1, 0);

            // Ad Soyad
            layout.Controls.Add(new Label { Text = "Ad Soyad:", AutoSize = true }, 0, 1);
            txtAdSoyad = new TextEdit { Width = 200 };
            layout.Controls.Add(txtAdSoyad, 1, 1);

            layout.Controls.Add(new Label { Text = "Rol:", AutoSize = true }, 0, 2);
            cmbRol = new System.Windows.Forms.ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            layout.Controls.Add(cmbRol, 1, 2);

            // İşletme
            layout.Controls.Add(new Label { Text = "İşletme:", AutoSize = true }, 0, 3);
            var lblBus = new Label { Text = SonucKullanici.IsletmeAdi ?? "Bilinmiyor", AutoSize = true };
            layout.Controls.Add(lblBus, 1, 3);

            // Şifre
            layout.Controls.Add(new Label { Text = "Şifre:", AutoSize = true }, 0, 4);
            txtSifre = new TextEdit { Width = 200 };
            txtSifre.Properties.PasswordChar = '*';
            layout.Controls.Add(txtSifre, 1, 4);
            
            // Butonlar
            var pnlButtons = new System.Windows.Forms.FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            btnIptal = new SimpleButton { Text = "İptal" };
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            
            btnKaydet = new SimpleButton { Text = "Kaydet" };
            btnKaydet.Click += BtnKaydet_Click;
            
            pnlButtons.Controls.Add(btnIptal);
            pnlButtons.Controls.Add(btnKaydet);
            
            layout.Controls.Add(pnlButtons, 1, 5);

            this.Controls.Add(layout);
        }

        private void RolleriYukle()
        {
            cmbRol.DataSource = _roller;
            cmbRol.DisplayMember = "Ad";
            cmbRol.ValueMember = "Id";
        }

        private void AlanlariDoldur()
        {
            txtKullaniciAdi.Text = SonucKullanici.KullaniciAdi;
            txtAdSoyad.Text = SonucKullanici.AdSoyad;
            cmbRol.SelectedValue = SonucKullanici.RolId;
            txtSifre.Text = ""; // Özeti gösterme
            txtSifre.Properties.NullText = "Değiştirmek için yazın";
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text))
            {
                XtraMessageBox.Show("Kullanıcı adı zorunludur.", "Uyarı");
                return;
            }

            // Eklerken şifre zorunlu. Düzenlerken isteğe bağlı.
            if (!_duzenlemeModu && string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                XtraMessageBox.Show("Şifre zorunludur.", "Uyarı");
                return;
            }

            SonucKullanici.KullaniciAdi = txtKullaniciAdi.Text.Trim();
            SonucKullanici.AdSoyad = txtAdSoyad.Text.Trim();
            
            if (cmbRol.SelectedItem is Rol rol)
            {
                SonucKullanici.RolId = rol.Id;
                SonucKullanici.RolAdi = rol.Ad; // Hemen görüntüleme için
            }

            if (!string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                SonucKullanici.SifreOzeti = txtSifre.Text.Trim(); 
            }
            else if (_duzenlemeModu)
            {
                SonucKullanici.SifreOzeti = null; // Şifreyi güncelleme sinyali
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
