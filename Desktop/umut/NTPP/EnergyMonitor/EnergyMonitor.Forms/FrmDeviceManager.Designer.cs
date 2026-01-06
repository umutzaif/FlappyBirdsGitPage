using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraTab;

namespace EnergyMonitor.Forms
{
    partial class FrmDeviceManager
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControlMain = new DevExpress.XtraTab.XtraTabControl();
            this.tabInventory = new DevExpress.XtraTab.XtraTabPage();
            this.tabIotCenter = new DevExpress.XtraTab.XtraTabPage();
            this.tabControlIot = new DevExpress.XtraTab.XtraTabControl();
            this.tabNetwork = new DevExpress.XtraTab.XtraTabPage();
            this.tabDeviceConfig = new DevExpress.XtraTab.XtraTabPage();
            this.splitContainerInventory = new DevExpress.XtraEditors.SplitContainerControl();
            this.pnlButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlInputsScroll = new System.Windows.Forms.Panel();

            this.gcDevices = new DevExpress.XtraGrid.GridControl();
            this.gvDevices = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pnlControls = new DevExpress.XtraEditors.PanelControl();
            this.grpDetails = new DevExpress.XtraEditors.GroupControl();
            
            // Existing controls
            this.lblId = new DevExpress.XtraEditors.LabelControl();
            this.txtId = new DevExpress.XtraEditors.TextEdit();
            this.lblName = new DevExpress.XtraEditors.LabelControl();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.lblBrand = new DevExpress.XtraEditors.LabelControl();
            this.txtBrand = new DevExpress.XtraEditors.TextEdit();
            this.lblModel = new DevExpress.XtraEditors.LabelControl();
            this.txtModel = new DevExpress.XtraEditors.TextEdit();
            this.lblType = new DevExpress.XtraEditors.LabelControl();
            this.cmbType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblMacAddress = new DevExpress.XtraEditors.LabelControl();
            this.txtMacAddress = new DevExpress.XtraEditors.TextEdit();
            this.lblVoltage = new DevExpress.XtraEditors.LabelControl();
            this.txtVoltage = new DevExpress.XtraEditors.SpinEdit();
            this.lblCurrent = new DevExpress.XtraEditors.LabelControl();
            this.txtCurrent = new DevExpress.XtraEditors.SpinEdit();
            this.lblWeight = new DevExpress.XtraEditors.LabelControl();
            this.txtWeight = new DevExpress.XtraEditors.SpinEdit();
            this.lblEntryDate = new DevExpress.XtraEditors.LabelControl();
            this.dtEntryDate = new DevExpress.XtraEditors.DateEdit();
            this.lblLastMaint = new DevExpress.XtraEditors.LabelControl();
            this.dtLastMaint = new DevExpress.XtraEditors.DateEdit();
            this.lblNextMaint = new DevExpress.XtraEditors.LabelControl();
            this.dtNextMaint = new DevExpress.XtraEditors.DateEdit();
            this.btnAdd = new DevExpress.XtraEditors.SimpleButton();
            this.btnUpdate = new DevExpress.XtraEditors.SimpleButton();
            this.btnDelete = new DevExpress.XtraEditors.SimpleButton();
            this.btnClear = new DevExpress.XtraEditors.SimpleButton();
            
            this.lblId.Text = "Cihaz ID:";
            this.lblName.Text = "Cihaz Adı:";
            this.lblBrand.Text = "Marka:";
            this.lblModel.Text = "Model:";
            this.lblType.Text = "Cihaz Türü:";
            this.lblMacAddress.Text = "MAC Adresi:";
            this.lblVoltage.Text = "Nominal Voltaj:";
            this.lblCurrent.Text = "Nominal Akım:";
            this.lblWeight.Text = "Ağırlık (kg):";
            this.lblEntryDate.Text = "Giriş Tarihi:";
            this.lblLastMaint.Text = "Son Bakım:";
            this.lblNextMaint.Text = "Gelecek Bakım:";

            this.btnAdd.Text = "Cihaz EKLE";
            this.btnUpdate.Text = "Cihaz GÜNCELLE";
            this.btnDelete.Text = "SİL";
            this.btnClear.Text = "TEMİZLE";

            ((System.ComponentModel.ISupportInitialize)(this.tabControlMain)).BeginInit();
            this.tabControlMain.SuspendLayout();
            this.tabInventory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcDevices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDevices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlControls)).BeginInit();
            this.pnlControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpDetails)).BeginInit();
            this.grpDetails.SuspendLayout();
            this.SuspendLayout();

            // 
            // tabControlMain
            // 
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
                this.tabInventory,
                this.tabIotCenter
            });
            
            // 
            // tabControlIot (Nested)
            // 
            this.tabIotCenter.Controls.Add(this.tabControlIot);
            this.tabIotCenter.Name = "tabIotCenter";
            this.tabIotCenter.Text = "IoT Kontrol Merkezi";
            
            this.tabControlIot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlIot.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
                this.tabNetwork,
                this.tabDeviceConfig
            });

            // 
            // tabInventory (Gelişmiş Cihaz Yöneticisi)
            // 
            this.tabInventory.Controls.Add(this.splitContainerInventory);
            this.tabInventory.Name = "tabInventory";
            this.tabInventory.Text = "Gelişmiş Cihaz Yöneticisi";

            // 
            // splitContainerInventory
            // 
            this.splitContainerInventory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerInventory.Location = new System.Drawing.Point(0, 0);
            this.splitContainerInventory.Name = "splitContainerInventory";
            this.splitContainerInventory.Panel1.Controls.Add(this.gcDevices);
            this.splitContainerInventory.Panel1.Text = "Panel1";
            this.splitContainerInventory.Panel2.Controls.Add(this.pnlControls);
            this.splitContainerInventory.Panel2.Text = "Panel2";
            this.splitContainerInventory.Size = new System.Drawing.Size(1100, 600);
            this.splitContainerInventory.SplitterPosition = 600;
            this.splitContainerInventory.TabIndex = 0;

            // 
            // tabNetwork (IoT Ağ Yöneticisi)
            // 
            this.tabNetwork.Name = "tabNetwork";
            this.tabNetwork.Text = "IoT Ağ Yöneticisi";
            InitializeNetworkTab();

            // 
            // tabDeviceConfig (IoT Aygıt Yöneticisi)
            // 
            this.tabDeviceConfig.Name = "tabDeviceConfig";
            this.tabDeviceConfig.Text = "IoT Aygıt Yöneticisi";
            InitializeDeviceConfigTab();

            // 
            // gcDevices
            // 
            this.gcDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcDevices.Location = new System.Drawing.Point(0, 0);
            this.gcDevices.MainView = this.gvDevices;
            this.gcDevices.Size = new System.Drawing.Size(600, 600);
            this.gcDevices.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { this.gvDevices });

            // 
            // gvDevices
            // 
            this.gvDevices.GridControl = this.gcDevices;
            this.gvDevices.OptionsBehavior.Editable = false;
            this.gvDevices.OptionsView.ShowGroupPanel = false;

            // 
            // pnlControls / grpDetails
            // 
            this.pnlControls.Controls.Add(this.grpDetails);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDetails.Text = "Cihaz Detayları";

            // (Manual positioning from previous version kept for brevity)
            LayoutInventoryControls();

            // 
            // Form Setup
            // 
            this.ClientSize = new System.Drawing.Size(1100, 700); // Larger size
            this.Controls.Add(this.tabControlMain);
            this.Name = "FrmDeviceManager";
            this.Text = "Enerlytics IoT & Cihaz Yönetim Merkezi";

            ((System.ComponentModel.ISupportInitialize)(this.tabControlMain)).EndInit();
            this.tabControlMain.ResumeLayout(false);
            this.tabInventory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcDevices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvDevices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlControls)).EndInit();
            this.pnlControls.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpDetails)).EndInit();
            this.grpDetails.ResumeLayout(false);
            this.grpDetails.PerformLayout();
            this.ResumeLayout(false);
        }

        private void LayoutInventoryControls()
        {
            this.grpDetails.Controls.Add(this.pnlInputsScroll);
            this.grpDetails.Controls.Add(this.pnlButtons);

            this.pnlInputsScroll.Dock = DockStyle.Fill;
            this.pnlInputsScroll.AutoScroll = true;

            this.pnlInputsScroll.Controls.AddRange(new Control[] {
                this.lblId, this.txtId, this.lblName, this.txtName, this.lblBrand, this.txtBrand,
                this.lblModel, this.txtModel, this.lblType, this.cmbType, this.lblMacAddress, this.txtMacAddress,
                this.lblVoltage, this.txtVoltage, this.lblCurrent, this.txtCurrent, this.lblWeight, this.txtWeight,
                this.lblEntryDate, this.dtEntryDate, this.lblLastMaint, this.dtLastMaint, this.lblNextMaint, this.dtNextMaint
            });

            this.pnlButtons.Dock = DockStyle.Bottom;
            this.pnlButtons.Height = 100;
            this.pnlButtons.Padding = new Padding(10, 5, 10, 5);
            this.pnlButtons.Controls.AddRange(new Control[] {
                this.btnAdd, this.btnUpdate, this.btnDelete, this.btnClear
            });

            this.pnlButtons.FlowDirection = FlowDirection.LeftToRight;
            this.pnlButtons.WrapContents = true;
            this.pnlButtons.BackColor = Color.FromArgb(240, 240, 240);

            int y = 20; int gap = 35; int labelX = 20; int controlX = 130; int width = 200;

            lblId.Location = new Point(labelX, y); txtId.Location = new Point(controlX, y - 3); txtId.Size = new Size(width, 22); y += gap;
            lblName.Location = new Point(labelX, y); txtName.Location = new Point(controlX, y - 3); txtName.Size = new Size(width, 22); y += gap;
            lblBrand.Location = new Point(labelX, y); txtBrand.Location = new Point(controlX, y - 3); txtBrand.Size = new Size(width, 22); y += gap;
            lblModel.Location = new Point(labelX, y); txtModel.Location = new Point(controlX, y - 3); txtModel.Size = new Size(width, 22); y += gap;
            lblType.Location = new Point(labelX, y); cmbType.Location = new Point(controlX, y - 3); cmbType.Size = new Size(width, 22); y += gap;
            lblMacAddress.Location = new Point(labelX, y); txtMacAddress.Location = new Point(controlX, y - 3); txtMacAddress.Size = new Size(width, 22); y += gap;
            lblVoltage.Location = new Point(labelX, y); txtVoltage.Location = new Point(controlX, y - 3); txtVoltage.Size = new Size(width, 22); y += gap;
            lblCurrent.Location = new Point(labelX, y); txtCurrent.Location = new Point(controlX, y - 3); txtCurrent.Size = new Size(width, 22); y += gap;
            lblWeight.Location = new Point(labelX, y); txtWeight.Location = new Point(controlX, y - 3); txtWeight.Size = new Size(width, 22); y += gap;
            lblEntryDate.Location = new Point(labelX, y); dtEntryDate.Location = new Point(controlX, y - 3); dtEntryDate.Size = new Size(width, 22); y += gap;
            lblLastMaint.Location = new Point(labelX, y); dtLastMaint.Location = new Point(controlX, y - 3); dtLastMaint.Size = new Size(width, 22); y += gap;
            lblNextMaint.Location = new Point(labelX, y); dtNextMaint.Location = new Point(controlX, y - 3); dtNextMaint.Size = new Size(width, 22); y += gap;

            btnAdd.Size = new Size(100, 35);
            btnUpdate.Size = new Size(100, 35);
            btnDelete.Size = new Size(130, 35);
            btnClear.Size = new Size(80, 35);
            
            foreach(Control c in pnlButtons.Controls) c.Margin = new Padding(3);
        }

        private void InitializeNetworkTab()
        {
            var grpWifi = new GroupControl { Text = "WiFi / Ağ Parametreleri", Location = new Point(20, 20), Size = new Size(400, 200) };
            lblSsid = new LabelControl { Text = "SSID:", Location = new Point(20, 40) };
            txtSsid = new TextEdit { Location = new Point(100, 37), Size = new Size(200, 22) };
            lblPass = new LabelControl { Text = "Parola:", Location = new Point(20, 80) };
            txtPass = new TextEdit { Location = new Point(100, 77), Size = new Size(200, 22), Properties = { UseSystemPasswordChar = true } };
            btnSaveWifi = new SimpleButton { Text = "Ağ Ayarlarını Master'a Gönder", Location = new Point(20, 130), Size = new Size(280, 40), ImageOptions = { Image = null } };
            
            grpWifi.Controls.AddRange(new Control[] { lblSsid, txtSsid, lblPass, txtPass, btnSaveWifi });
            this.tabNetwork.Controls.Add(grpWifi);

            var grpScan = new GroupControl { Text = "Ağdaki Aktif Cihazlar", Location = new Point(440, 20), Size = new Size(600, 500), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            gcNetScan = new DevExpress.XtraGrid.GridControl { Dock = DockStyle.Fill };
            gvNetScan = new DevExpress.XtraGrid.Views.Grid.GridView { GridControl = gcNetScan };
            gcNetScan.MainView = gvNetScan;
            btnScan = new SimpleButton { Text = "Ağı Tara / Test Bağlantısı", Location = new Point(20, 20), Size = new Size(150, 30), Dock = DockStyle.Bottom };
            
            grpScan.Controls.Add(gcNetScan);
            grpScan.Controls.Add(btnScan);
            this.tabNetwork.Controls.Add(grpScan);
        }

        private void InitializeDeviceConfigTab()
        {
            var grpCfg = new GroupControl { Text = "Aygıt / Node Yapılandırması", Location = new Point(20, 20), Size = new Size(500, 400) };
            
            lblCfgDevice = new LabelControl { Text = "Aygıt Seçin:", Location = new Point(20, 40) };
            lookCfgDevice = new LookUpEdit { Location = new Point(120, 37), Size = new Size(300, 22) };
            
            lblCfgPeriod = new LabelControl { Text = "Veri Periyodu (ms):", Location = new Point(20, 80) };
            numCfgPeriod = new SpinEdit { Location = new Point(120, 77), Size = new Size(100, 22), Properties = { Increment = 100, MinValue = 100, MaxValue = 60000 } };
            
            lblCfgSensors = new LabelControl { Text = "Aktif Sensörler:", Location = new Point(20, 120) };
            chkCfgSensors = new CheckedComboBoxEdit { Location = new Point(120, 117), Size = new Size(300, 22) };
            chkCfgSensors.Properties.Items.AddRange(new string[] { "Voltaj", "Akım", "Güç Faktörü", "Sıcaklık", "Titreşim", "RPM" });

            btnCfgWrite = new SimpleButton { Text = "Donanıma Yaz (Flash)", Location = new Point(120, 170), Size = new Size(140, 40), Appearance = { BackColor = Color.DarkGreen, ForeColor = Color.White } };
            btnCfgTest = new SimpleButton { Text = "PING / Test", Location = new Point(270, 170), Size = new Size(140, 40) };

            grpCfg.Controls.AddRange(new Control[] { lblCfgDevice, lookCfgDevice, lblCfgPeriod, numCfgPeriod, lblCfgSensors, chkCfgSensors, btnCfgWrite, btnCfgTest });
            this.tabDeviceConfig.Controls.Add(grpCfg);
        }

        private DevExpress.XtraTab.XtraTabControl tabControlMain;
        private DevExpress.XtraTab.XtraTabPage tabInventory;
        private DevExpress.XtraTab.XtraTabPage tabIotCenter;
        private DevExpress.XtraTab.XtraTabControl tabControlIot;
        private DevExpress.XtraTab.XtraTabPage tabNetwork;
        private DevExpress.XtraTab.XtraTabPage tabDeviceConfig;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerInventory;
        private System.Windows.Forms.FlowLayoutPanel pnlButtons;
        private System.Windows.Forms.Panel pnlInputsScroll;

        // Inventory
        private DevExpress.XtraGrid.GridControl gcDevices;
        private DevExpress.XtraGrid.Views.Grid.GridView gvDevices;
        private DevExpress.XtraEditors.PanelControl pnlControls;
        private DevExpress.XtraEditors.GroupControl grpDetails;
        private DevExpress.XtraEditors.LabelControl lblId;
        private DevExpress.XtraEditors.TextEdit txtId;
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl lblBrand;
        private DevExpress.XtraEditors.TextEdit txtBrand;
        private DevExpress.XtraEditors.LabelControl lblModel;
        private DevExpress.XtraEditors.TextEdit txtModel;
        private DevExpress.XtraEditors.LabelControl lblType;
        private DevExpress.XtraEditors.ComboBoxEdit cmbType;
        private DevExpress.XtraEditors.LabelControl lblMacAddress;
        private DevExpress.XtraEditors.TextEdit txtMacAddress;
        private DevExpress.XtraEditors.LabelControl lblVoltage;
        private DevExpress.XtraEditors.SpinEdit txtVoltage;
        private DevExpress.XtraEditors.LabelControl lblCurrent;
        private DevExpress.XtraEditors.SpinEdit txtCurrent;
        private DevExpress.XtraEditors.LabelControl lblWeight;
        private DevExpress.XtraEditors.SpinEdit txtWeight;
        private DevExpress.XtraEditors.LabelControl lblEntryDate;
        private DevExpress.XtraEditors.DateEdit dtEntryDate;
        private DevExpress.XtraEditors.LabelControl lblLastMaint;
        private DevExpress.XtraEditors.DateEdit dtLastMaint;
        private DevExpress.XtraEditors.LabelControl lblNextMaint;
        private DevExpress.XtraEditors.DateEdit dtNextMaint;
        private DevExpress.XtraEditors.SimpleButton btnAdd;
        private DevExpress.XtraEditors.SimpleButton btnUpdate;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        private DevExpress.XtraEditors.SimpleButton btnClear;

        // Network
        private LabelControl lblSsid;
        private TextEdit txtSsid;
        private LabelControl lblPass;
        private TextEdit txtPass;
        private SimpleButton btnSaveWifi;
        private DevExpress.XtraGrid.GridControl gcNetScan;
        private DevExpress.XtraGrid.Views.Grid.GridView gvNetScan;
        private SimpleButton btnScan;

        // Device Config
        private LabelControl lblCfgDevice;
        private LookUpEdit lookCfgDevice;
        private LabelControl lblCfgPeriod;
        private SpinEdit numCfgPeriod;
        private LabelControl lblCfgSensors;
        private CheckedComboBoxEdit chkCfgSensors;
        private SimpleButton btnCfgWrite;
        private SimpleButton btnCfgTest;
    }
}
