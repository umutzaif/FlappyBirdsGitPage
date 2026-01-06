using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace EnergyMonitor.Forms
{
    public partial class FrmChangePassword : XtraForm
    {
        public string YeniSifre { get; private set; }

        private TextEdit txtYeniSifre;
        private TextEdit txtSifreOnay;
        private SimpleButton btnKaydet;

        public FrmChangePassword()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Şifre Değiştir";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), RowCount = 3 };
            
            layout.Controls.Add(new LabelControl { Text = "Yeni Şifre:" }, 0, 0);
            txtYeniSifre = new TextEdit { Dock = DockStyle.Fill, Properties = { UseSystemPasswordChar = true } };
            layout.Controls.Add(txtYeniSifre, 0, 1);

            layout.Controls.Add(new LabelControl { Text = "Tekrar:" }, 0, 2);
            txtSifreOnay = new TextEdit { Dock = DockStyle.Fill, Properties = { UseSystemPasswordChar = true } };
            layout.Controls.Add(txtSifreOnay, 0, 3);

            btnKaydet = new SimpleButton { Text = "Kaydet", Dock = DockStyle.Bottom };
            btnKaydet.Click += BtnKaydet_Click;

            this.Controls.Add(layout);
            this.Controls.Add(btnKaydet);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            var p1 = txtYeniSifre.Text;
            var p2 = txtSifreOnay.Text;

            if (string.IsNullOrWhiteSpace(p1))
            {
                XtraMessageBox.Show("Şifre boş olamaz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (p1 != p2)
            {
                XtraMessageBox.Show("Şifreler eşleşmiyor.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            YeniSifre = p1;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
