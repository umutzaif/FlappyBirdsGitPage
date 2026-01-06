using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Forms
{
    public partial class FrmReportOptions : DevExpress.XtraEditors.XtraForm
    {
        public DateTime BaslangicTarihi => dtpStartDate.Value;
        public DateTime BitisTarihi => dtpEndDate.Value;
        public bool LoglariDahilEt => chkIncludeLogs.Checked;
        public int? SeciliCihazId => (cmbDevices.SelectedItem as ListeOgeleri)?.Id;

        // Yardımcı Sınıf
        private class ListeOgeleri 
        { 
            public int Id { get; set; } 
            public string Ad { get; set; } 
            public override string ToString() => Ad;
        }

        public FrmReportOptions(List<Cihaz> cihazlar)
        {
            InitializeComponent();
            CihazlariYukle(cihazlar);
        }

        private void CihazlariYukle(List<Cihaz> cihazlar)
        {
            cmbDevices.Items.Clear();
            cmbDevices.Items.Add(new ListeOgeleri { Id = 0, Ad = "Tüm Cihazlar" });
            foreach (var c in cihazlar)
            {
                cmbDevices.Items.Add(new ListeOgeleri { Id = c.Id, Ad = c.Ad });
            }
            cmbDevices.SelectedIndex = 0;
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnIptal_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // DESIGNER CODE
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.ComboBox cmbDevices;
        private System.Windows.Forms.CheckBox chkIncludeLogs;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1, label2, label3;

        private void InitializeComponent()
        {
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.cmbDevices = new System.Windows.Forms.ComboBox();
            this.chkIncludeLogs = new System.Windows.Forms.CheckBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // Label 1
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Text = "Başlangıç:";
            
            // dtpStartDate
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new System.Drawing.Point(100, 15);
            this.dtpStartDate.Size = new System.Drawing.Size(120, 21);
            this.dtpStartDate.Value = DateTime.Now.AddDays(-7);

            // Label 2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 50);
            this.label2.Text = "Bitiş:";

            // dtpEndDate
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpEndDate.Location = new System.Drawing.Point(100, 45);
            this.dtpEndDate.Size = new System.Drawing.Size(120, 21);

            // Label 3
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 80);
            this.label3.Text = "Cihaz:";

            // cmbDevices
            this.cmbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDevices.Location = new System.Drawing.Point(100, 75);
            this.cmbDevices.Size = new System.Drawing.Size(180, 21);

            // chkIncludeLogs
            this.chkIncludeLogs.AutoSize = true;
            this.chkIncludeLogs.Location = new System.Drawing.Point(23, 110);
            this.chkIncludeLogs.Text = "Log Kayıtlarını Dahil Et (Detaylı)";
            this.chkIncludeLogs.Checked = false;

            // btnGenerate
            this.btnGenerate.Location = new System.Drawing.Point(100, 150);
            this.btnGenerate.Text = "Rapor Oluştur";
            this.btnGenerate.Size = new System.Drawing.Size(100, 30);
            this.btnGenerate.Click += new System.EventHandler(this.btnEkle_Click);
            this.btnGenerate.BackColor = System.Drawing.Color.LightGreen;

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(210, 150);
            this.btnCancel.Text = "İptal";
            this.btnCancel.Click += new System.EventHandler(this.btnIptal_Click);

            // Form
            this.ClientSize = new System.Drawing.Size(350, 200);
            this.Controls.Add(label1);
            this.Controls.Add(dtpStartDate);
            this.Controls.Add(label2);
            this.Controls.Add(dtpEndDate);
            this.Controls.Add(label3);
            this.Controls.Add(cmbDevices);
            this.Controls.Add(chkIncludeLogs);
            this.Controls.Add(btnGenerate);
            this.Controls.Add(btnCancel);
            this.Text = "Rapor Seçenekleri";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
