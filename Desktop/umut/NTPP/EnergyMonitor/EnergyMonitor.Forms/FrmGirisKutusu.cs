using System;
using System.Windows.Forms;
using System.Drawing;

namespace EnergyMonitor.Forms
{
    public class FrmGirisKutusu : Form
    {
        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOk;
        private Button btnCancel;
        
        public string GirisDegeri => txtInput.Text;

        public FrmGirisKutusu(string mesaj, string baslik, string varsayilanDeger = "")
        {
            this.Text = baslik;
            this.Size = new Size(350, 160);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MaximizeBox = false;

            lblPrompt = new Label { Text = mesaj, Location = new Point(10, 10), AutoSize = true };
            txtInput = new TextBox { Text = varsayilanDeger, Location = new Point(10, 40), Width = 310 };
            btnOk = new Button { Text = "Tamam", Location = new Point(160, 80), DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "İptal", Location = new Point(240, 80), DialogResult = DialogResult.Cancel };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        public static string Goster(string mesaj, string baslik, string varsayilanDeger = "")
        {
            using (var form = new FrmGirisKutusu(mesaj, baslik, varsayilanDeger))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.GirisDegeri;
                }
            }
            return null; // İptal
        }
    }
}
