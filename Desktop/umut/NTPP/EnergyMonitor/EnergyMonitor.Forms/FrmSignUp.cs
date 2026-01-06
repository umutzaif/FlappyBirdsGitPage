using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Forms
{
    public partial class FrmSignUp : XtraForm
    {
        public Kullanici SonucKullanici { get; private set; }
        private readonly List<IsletmeBilgisi> _isletmeler;

        private TextEdit txtKullaniciAdi;
        private TextEdit txtSifre;
        private TextEdit txtAdSoyad;
        private System.Windows.Forms.ComboBox cmbIsletme;
        private SimpleButton btnGonder;
        private SimpleButton btnIptal;

        public FrmSignUp(List<IsletmeBilgisi> isletmeler)
        {
            _isletmeler = isletmeler;
            InitializeComponent();
            this.Text = "Hesap Oluşturma İsteği";
            IsletmeleriYukle();
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 6, ColumnCount = 2 };
            
            layout.Controls.Add(new Label { Text = "İşletme Seçiniz:", AutoSize = true }, 0, 0);
            cmbIsletme = new System.Windows.Forms.ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            layout.Controls.Add(cmbIsletme, 1, 0);

            layout.Controls.Add(new Label { Text = "Kullanıcı Adı:", AutoSize = true }, 0, 1);
            txtKullaniciAdi = new TextEdit { Width = 200 };
            layout.Controls.Add(txtKullaniciAdi, 1, 1);

            layout.Controls.Add(new Label { Text = "Ad Soyad:", AutoSize = true }, 0, 2);
            txtAdSoyad = new TextEdit { Width = 200 };
            layout.Controls.Add(txtAdSoyad, 1, 2);

            layout.Controls.Add(new Label { Text = "Şifre:", AutoSize = true }, 0, 3);
            txtSifre = new TextEdit { Width = 200, Properties = { UseSystemPasswordChar = true } };
            layout.Controls.Add(txtSifre, 1, 3);

            var lblInfo = new LabelControl { 
                Text = "Not: Hesabınız Admin/IT tarafından onaylandıktan sonra aktifleşecektir.", 
                AutoSizeMode = LabelAutoSizeMode.Vertical, 
                Dock = DockStyle.Fill,
                Appearance = { ForeColor = System.Drawing.Color.Gray, Font = new System.Drawing.Font("Segoe UI", 8) }
            };
            layout.Controls.Add(lblInfo, 0, 4);
            layout.SetColumnSpan(lblInfo, 2);

            var pnlButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            btnIptal = new SimpleButton { Text = "İptal" };
            btnIptal.Click += (s, e) => this.Close();
            btnGonder = new SimpleButton { Text = "İstek Gönder" };
            btnGonder.Click += BtnGonder_Click;
            pnlButtons.Controls.Add(btnIptal);
            pnlButtons.Controls.Add(btnGonder);
            layout.Controls.Add(pnlButtons, 1, 5);

            this.Controls.Add(layout);
        }

        private void IsletmeleriYukle()
        {
            cmbIsletme.DataSource = _isletmeler;
            cmbIsletme.DisplayMember = "SirketAdi";
            cmbIsletme.ValueMember = "Id";
        }

        private void BtnGonder_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKullaniciAdi.Text) || string.IsNullOrWhiteSpace(txtSifre.Text) || cmbIsletme.SelectedValue == null)
            {
                XtraMessageBox.Show("Lütfen tüm alanları doldurunuz.", "Uyarı");
                return;
            }

            SonucKullanici = new Kullanici
            {
                KullaniciAdi = txtKullaniciAdi.Text.Trim(),
                AdSoyad = txtAdSoyad.Text.Trim(),
                SifreOzeti = txtSifre.Text.Trim(), 
                IsletmeId = (int)cmbIsletme.SelectedValue,
                AktifMi = false,
                RolAdi = "Izleme_Personeli" 
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
