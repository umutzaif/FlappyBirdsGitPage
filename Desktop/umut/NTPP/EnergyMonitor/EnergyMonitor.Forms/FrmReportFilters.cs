using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Forms
{
    public partial class FrmReportFilters : XtraForm
    {
        public DateTime BaslangicTarihi => dtStart.DateTime;
        public DateTime BitisTarihi => dtEnd.DateTime;
        public List<int> SeciliCihazIdleri 
        {
            get 
            {
                var val = chkDeviceList.EditValue?.ToString();
                if (string.IsNullOrEmpty(val)) return new List<int>();
                return val.Split(',').Select(x => int.Parse(x.Trim())).ToList();
            }
        }
        public bool LoglariDahilEt => chkLogs.Checked;

        private CheckedComboBoxEdit chkDeviceList;
        private DateEdit dtStart;
        private DateEdit dtEnd;
        private CheckEdit chkLogs;
        private SimpleButton btnOnayla;
        private SimpleButton btnIptal;

        public FrmReportFilters(List<Cihaz> cihazlar, string raporBasligi, bool logSeceneginiGoster = true)
        {
            InitializeComponent();
            this.Text = $"{raporBasligi} - Parametre Seçimi";
            
            // Cihazları doldur - Sadece ID, Ad, Marka
            var gosterimListesi = cihazlar.Select(d => new { d.Id, d.Ad, d.Marka }).ToList();
            chkDeviceList.Properties.DataSource = gosterimListesi;
            chkDeviceList.Properties.DisplayMember = "Ad";
            chkDeviceList.Properties.ValueMember = "Id";
            
            chkDeviceList.CheckAll(); 

            dtStart.DateTime = DateTime.Today.AddDays(-7);
            dtEnd.DateTime = DateTime.Now;
            chkLogs.Visible = logSeceneginiGoster;
        }

        private void InitializeComponent()
        {
            this.chkDeviceList = new CheckedComboBoxEdit();
            this.dtStart = new DateEdit();
            this.dtEnd = new DateEdit();
            this.chkLogs = new CheckEdit();
            this.btnOnayla = new SimpleButton();
            this.btnIptal = new SimpleButton();

            var lblDevice = new LabelControl { Text = "Cihaz Seçimi:", Location = new Point(20, 25) };
            chkDeviceList.Location = new Point(130, 22);
            chkDeviceList.Size = new Size(200, 22);
            chkDeviceList.Properties.SelectAllItemCaption = "TÜMÜNÜ SEÇ";
            chkDeviceList.Properties.SeparatorChar = ',';

            var lblStart = new LabelControl { Text = "Başlangıç Tarihi:", Location = new Point(20, 55) };
            dtStart.Location = new Point(130, 52);
            dtStart.Size = new Size(200, 22);

            var lblEnd = new LabelControl { Text = "Bitiş Tarihi:", Location = new Point(20, 85) };
            dtEnd.Location = new Point(130, 82);
            dtEnd.Size = new Size(200, 22);

            chkLogs.Text = "Detaylı Logları Dahil Et (PDF Boyutu Artabilir)";
            chkLogs.Location = new Point(130, 115);
            chkLogs.Size = new Size(250, 22);

            btnOnayla.Text = "Raporu Oluştur";
            btnOnayla.Location = new Point(130, 150);
            btnOnayla.Size = new Size(110, 35);
            btnOnayla.Appearance.BackColor = Color.LightSeaGreen;
            btnOnayla.Click += (s, e) => { 
                if (SeciliCihazIdleri.Count == 0) {
                    XtraMessageBox.Show("Lütfen en az bir cihaz seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                this.DialogResult = DialogResult.OK; this.Close(); 
            };

            btnIptal.Text = "İptal";
            btnIptal.Location = new Point(250, 150);
            btnIptal.Size = new Size(80, 35);
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.ClientSize = new Size(380, 210);
            this.Controls.AddRange(new Control[] { lblDevice, chkDeviceList, lblStart, dtStart, lblEnd, dtEnd, chkLogs, btnOnayla, btnIptal });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }
    }
}
