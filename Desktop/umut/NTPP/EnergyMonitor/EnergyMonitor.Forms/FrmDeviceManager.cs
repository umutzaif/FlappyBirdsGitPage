using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;
using EnergyMonitor.Service;
using DevExpress.XtraEditors;

namespace EnergyMonitor.Forms
{
    public partial class FrmDeviceManager : XtraForm
    {
        private readonly ICihazServisi _cihazServisi;
        private readonly ISeriPortServisi _seriPortServisi;

        private int _mevcutGorunumDurumu = 0; // 0: Lobi, 1: Aktif, 3: Çöp
        private SimpleButton btnViewLobby;
        private SimpleButton btnViewActive;
        private SimpleButton btnViewTrash;
        private SimpleButton btnRestore;
        private SimpleButton btnHardwareSync; 

        public FrmDeviceManager(string baglantiCumlesi, ISeriPortServisi seriPortServisi)
        {
            InitializeComponent();
            _cihazServisi = new SCihaz(baglantiCumlesi);
            _seriPortServisi = seriPortServisi;

            // Donanım geri bildirimlerine abone ol
            _seriPortServisi.VeriAlindi += Donanim_VeriAlindi;

            // 1. Temizlik ve Hazırlık
            this.Controls.Clear(); // Designer'dan gelen HER ŞEYİ temizle
            
            // 2. Global Başlık Paneli
            var pnlGlobalHeader = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 60, // Biraz daha genişletelim (nefes payı)
                BackColor = Color.FromArgb(10, 25, 47), 
                Padding = new Padding(10) 
            };
            
            var lblHeader = new LabelControl 
            { 
                Text = "ENERLYTICS IoT KONTROL MERKEZİ", 
                Dock = DockStyle.Fill, 
                Appearance = { 
                    Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                    ForeColor = Color.Turquoise, 
                    TextOptions = { HAlignment = DevExpress.Utils.HorzAlignment.Center, VAlignment = DevExpress.Utils.VertAlignment.Center } 
                }
            };

            btnHardwareSync = new SimpleButton { Text = "Donanıma Kaydet", Dock = DockStyle.Right, Width = 140 };
            btnHardwareSync.Appearance.BackColor = Color.LightSeaGreen;
            btnHardwareSync.Click += (s, e) => DonanimSenkronizasyonuYonet();

            pnlGlobalHeader.Controls.Add(lblHeader);
            pnlGlobalHeader.Controls.Add(btnHardwareSync);
            
            // 3. Sekme Kontrolleri Yapılandırması
            // Designer'dan gelen tabControlMain'i tekrar yapılandır
            this.tabControlMain.Dock = DockStyle.Fill;
            this.tabControlMain.ShowTabHeader = DevExpress.Utils.DefaultBoolean.True;
            this.tabControlMain.AppearancePage.Header.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Daha belirgin sekmeler
            
            this.tabControlIot.Dock = DockStyle.Fill;
            this.tabControlIot.ShowTabHeader = DevExpress.Utils.DefaultBoolean.True;
            this.tabControlIot.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Top;

            // Kontrolleri forma ekle (Sıralama önemli: Fill olan en son!)
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(pnlGlobalHeader);
            
            pnlGlobalHeader.BringToFront(); // Başlık her zaman en üstte (Top)
            this.tabControlMain.BringToFront(); // Sekmeler panelin hemen altında kalacak şekilde

            // 4. Envanter Sekmesine Özel Filtre Butonları (Sekme İçinde)
            var pnlInventoryFilters = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.WhiteSmoke, Padding = new Padding(5) };
            
            btnViewLobby = new SimpleButton { Text = "Lobi", Dock = DockStyle.Left, Width = 110 };
            btnViewActive = new SimpleButton { Text = "Aktif Cihazlar", Dock = DockStyle.Left, Width = 110 };
            btnViewTrash = new SimpleButton { Text = "Çöp Kutusu", Dock = DockStyle.Left, Width = 110 };
            btnRestore = new SimpleButton { Text = "Seçileni Geri Yükle", Dock = DockStyle.Right, Width = 140, Visible = false };
            btnRestore.Click += async (s, e) => await CoptenGeriYukle();

            pnlInventoryFilters.Controls.Add(btnViewLobby);
            pnlInventoryFilters.Controls.Add(btnViewActive);
            pnlInventoryFilters.Controls.Add(btnViewTrash);
            pnlInventoryFilters.Controls.Add(btnRestore);

            this.tabInventory.Controls.Add(pnlInventoryFilters);
            pnlInventoryFilters.SendToBack(); // Sekme içindeki grid/panel ile hizalanması için

            // Kategori butonu tıklamaları
            btnViewLobby.Click += (s, e) => GorunumuDegistir(0);
            btnViewActive.Click += (s, e) => GorunumuDegistir(1);
            btnViewTrash.Click += (s, e) => GorunumuDegistir(3);

            // Event Handlers
            this.FormClosing += (s, e) => _seriPortServisi.VeriAlindi -= Donanim_VeriAlindi;
            this.Load += FrmDeviceManager_Load;
            
            // Diğer Eventler...
            this.gvDevices.FocusedRowChanged += GvCihazlar_OdaklanilanSatirDegisti;
            this.btnAdd.Click += BtnEkle_Tikla;
            this.btnUpdate.Click += BtnGuncelle_Tikla;
            this.btnDelete.Click += BtnSil_Tikla;
            this.btnClear.Click += BtnTemizle_Tikla;

            // Buton metinlerini garantiye al
            this.btnAdd.Text = "EKLE";
            this.btnUpdate.Text = "GÜNCELLE";
            this.btnClear.Text = "TEMİZLE";
            this.btnDelete.Text = "SİL (Çöp Kutusuna)";
            
            // IoT Handlers
            this.btnSaveWifi.Click += BtnWifiKaydet_Tikla;
            this.btnScan.Click += BtnArama_Tikla;
            this.btnCfgWrite.Click += BtnYapilandirmaYaz_Tikla;
            this.btnCfgTest.Click += BtnYapilandirmaTesti_Tikla;
            this.lookCfgDevice.EditValueChanged += LookYapilandirmaCihazi_Degisti;

            // Cihaz Türlerini Doldur
            cmbType.Properties.Items.Clear();
            foreach (var turAdi in Enum.GetNames(typeof(CihazTuru)))
                cmbType.Properties.Items.Add(turAdi);
            cmbType.SelectedIndex = 0;
        }

        private async void Donanim_VeriAlindi(object? sender, string veri)
        {
            if (this.InvokeRequired) { this.Invoke(new Action<object?, string>(Donanim_VeriAlindi), sender, veri); return; }

            if (veri.StartsWith("REG_OK:"))
            {
                int id = int.Parse(veri.Split(':')[1]);
                await _cihazServisi.CihazDurumuGuncelleAsync(id, 1); // Aktif'e taşı
                XtraMessageBox.Show($"Cihaz (ID:{id}) başarıyla donanıma kaydedildi ve aktif edildi.");
                await GridiYenile();
            }
            else if (veri.StartsWith("DEL_OK:"))
            {
                int id = int.Parse(veri.Split(':')[1]);
                await _cihazServisi.CihazSilAsync(id); // Kalıcı Veritabanı silme
                XtraMessageBox.Show($"Cihaz (ID:{id}) donanımdan silindi ve sistemden kaldırıldı.");
                BtnTemizle_Tikla(null!, EventArgs.Empty);
                await GridiYenile();
            }
            else if (veri.StartsWith("WIFI_OK"))
            {
                XtraMessageBox.Show("WiFi ayarları Master cihaz tarafından başarıyla uygulandı.", "IoT Ağ Bilgisi");
            }
            else if (veri.StartsWith("CFG_OK:"))
            {
                XtraMessageBox.Show($"Cihaz yapılandırması başarıyla donanıma yazıldı: {veri}", "IoT Donanım Bilgisi");
            }
            else if (veri.StartsWith("PONG:"))
            {
                XtraMessageBox.Show($"Cihaz AKTİF: {veri}", "Bağlantı Testi (PING)");
            }
        }

        private void DonanimSenkronizasyonuYonet()
        {
             if (!int.TryParse(txtId.Text, out int id) || id <= 0) 
             {
                 XtraMessageBox.Show("Lütfen listeden bir cihaz seçin.");
                 return;
             }

             if (!_seriPortServisi.AcikMi)
             {
                 XtraMessageBox.Show("IoT donanımı bağlı değil! Lütfen Dashboard üzerinden portu bağlayın.", "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 return;
             }

             if (_mevcutGorunumDurumu == 0) // Lobi -> Aktif'e
             {
                 _seriPortServisi.VeriYaz($"REG_ID:{id}|MAC:{txtMacAddress.Text}");
                 XtraMessageBox.Show("Kayıt komutu gönderildi. Cihazdan onay bekleniyor...");
             }
             else if (_mevcutGorunumDurumu == 3) // Çöp -> Kalıcı Silme
             {
                 _seriPortServisi.VeriYaz($"DEL_ID:{id}");
                 XtraMessageBox.Show("Silme komutu gönderildi. Cihazdan onay bekleniyor...");
             }
        }

        private void GorunumuDegistir(int durum)
        {
            _mevcutGorunumDurumu = durum;
            btnRestore.Visible = (durum == 3);
            btnDelete.Text = (durum == 3) ? "Kalıcı Sil (Donanımdan)" : "SİL (Çöp Kutusuna)";
            btnViewLobby.Text = "Lobi"; 
            btnViewActive.Text = "Aktif Cihazlar";
            btnViewTrash.Text = "Çöp Kutusu";
            
            // Donanım Senkronizasyon butonu metni/görünürlüğü
            if (durum == 0) { btnHardwareSync.Text = "Donanıma Kaydet"; btnHardwareSync.Visible = true; }
            else if (durum == 3) { btnHardwareSync.Text = "Donanımdan Sil"; btnHardwareSync.Visible = true; }
            else btnHardwareSync.Visible = false;

            _ = GridiYenile();
        }

        private async System.Threading.Tasks.Task CoptenGeriYukle()
        {
             if (int.TryParse(txtId.Text, out int id) && id > 0)
             {
                 await _cihazServisi.CihazDurumuGuncelleAsync(id, 0); // Lobiye geri yükle
                 XtraMessageBox.Show("Cihaz lobiye geri yüklendi.");
                 await GridiYenile();
             }
        }

        private async void FrmDeviceManager_Load(object? sender, EventArgs e)
        {
            GorunumuDegistir(0); // Varsayılan: Lobi
            await GridiYenile();
            await IoTLookupYukle();
            
            // Yeni özellikleri göstermek için IoT Ağ Sekmesini vurgula
            tabControlMain.SelectedTabPageIndex = 1; 
        }

        private async Task IoTLookupYukle()
        {
            var cihazlar = await _cihazServisi.TumCihazlariGetirAsync();
            var aktifler = cihazlar.Where(x => x.Durum == 1 || x.Durum == 2).ToList();
            lookCfgDevice.Properties.DataSource = aktifler;
            lookCfgDevice.Properties.DisplayMember = "Ad";
            lookCfgDevice.Properties.ValueMember = "Id";
        }

        private async System.Threading.Tasks.Task GridiYenile()
        {
            try
            {
                var cihazlar = await _cihazServisi.TumCihazlariGetirAsync();
                
                var filtrelenmis = cihazlar.Where(d => 
                {
                    if (_mevcutGorunumDurumu == 1) return d.Durum == 1 || d.Durum == 2; // Aktif + Pasif
                    return d.Durum == _mevcutGorunumDurumu;
                }).ToList();
                
                gcDevices.DataSource = filtrelenmis;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Veri yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GvCihazlar_OdaklanilanSatirDegisti(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var cihaz = gvDevices.GetFocusedRow() as Cihaz;
            if (cihaz != null)
            {
                txtId.Text = cihaz.Id.ToString();
                txtName.Text = cihaz.Ad;
                txtBrand.Text = cihaz.Marka;
                txtModel.Text = cihaz.Model;
                txtMacAddress.Text = cihaz.MacAdresi;
                txtVoltage.Value = cihaz.GerilimDegeri.GetValueOrDefault();
                txtCurrent.Value = cihaz.AkimDegeri.GetValueOrDefault();
                txtWeight.Value = cihaz.Agirlik.GetValueOrDefault();
                dtEntryDate.EditValue = (object?)cihaz.GirisTarihi ?? DBNull.Value;
                dtLastMaint.EditValue = (object?)cihaz.SonBakim ?? DBNull.Value;
                dtNextMaint.EditValue = (object?)cihaz.SonrakiBakim ?? DBNull.Value;
                cmbType.SelectedItem = cihaz.Tur.ToString();
            }
        }

        private void BtnTemizle_Tikla(object? sender, EventArgs e)
        {
            txtId.Text = "";
            txtName.Text = "";
            txtBrand.Text = "";
            txtModel.Text = "";
            txtMacAddress.Text = "";
            txtVoltage.Value = 0;
            txtCurrent.Value = 0;
            txtWeight.Value = 0;
            dtEntryDate.EditValue = DateTime.Now;
            dtLastMaint.EditValue = DBNull.Value;
            dtNextMaint.EditValue = DBNull.Value;
            cmbType.SelectedIndex = 0;

            // Hatalı satır: gcDevices.MainView.ClearSelection();
            // Doğru kullanım:
            var gridView = gcDevices.MainView as DevExpress.XtraGrid.Views.Grid.GridView;
            if (gridView != null)
                gridView.ClearSelection();
        }

        private void BtnEkle_Tikla(object? sender, EventArgs e)
        {
            _ = BtnEkle_Async(sender, e);
        }

        private void BtnGuncelle_Tikla(object? sender, EventArgs e)
        {
            _ = BtnGuncelle_Async(sender, e);
        }

        private async System.Threading.Tasks.Task BtnEkle_Async(object? sender, EventArgs e)
        {
            try
            {
                var cihaz = CihaziFormdanAl();
                if (string.IsNullOrEmpty(cihaz.Ad))
                {
                    XtraMessageBox.Show("Cihaz Adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _cihazServisi.CihazEkleAsync(cihaz);
                XtraMessageBox.Show("Yeni cihaz eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await GridiYenile();
                BtnTemizle_Tikla(null, null);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Ekleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async System.Threading.Tasks.Task BtnGuncelle_Async(object? sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(txtId.Text, out int id) || id <= 0)
                {
                    XtraMessageBox.Show("Lütfen güncellenecek bir cihaz seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var cihaz = CihaziFormdanAl();
                cihaz.Id = id;

                if (string.IsNullOrEmpty(cihaz.Ad))
                {
                    XtraMessageBox.Show("Cihaz Adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _cihazServisi.CihazGuncelleAsync(cihaz);
                XtraMessageBox.Show("Cihaz güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await GridiYenile();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Güncelleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Cihaz CihaziFormdanAl()
        {
            return new Cihaz
            {
                Ad = txtName.Text.Trim(),
                Marka = txtBrand.Text.Trim(),
                Model = txtModel.Text.Trim(),
                MacAdresi = txtMacAddress.Text.Trim(),
                GerilimDegeri = txtVoltage.Value,
                AkimDegeri = txtCurrent.Value,
                Agirlik = txtWeight.Value,
                GirisTarihi = dtEntryDate.EditValue as DateTime?,
                SonBakim = dtLastMaint.EditValue as DateTime?,
                SonrakiBakim = dtNextMaint.EditValue as DateTime?,
                Tur = (CihazTuru)Enum.Parse(typeof(CihazTuru), cmbType.SelectedItem.ToString()),
                Durum = 0 
            };
        }

        private async void BtnSil_Tikla(object? sender, EventArgs e)
        {
            if (int.TryParse(txtId.Text, out int id) && id > 0)
            {
                if (XtraMessageBox.Show("Bu işlem cihazı silecektir. Emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        if (_mevcutGorunumDurumu == 3) // Çöp Görünümü -> Kalıcı Silme
                        {
                            await _cihazServisi.CihazSilAsync(id);
                             XtraMessageBox.Show("Cihaz kalıcı olarak silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else // Aktif/Lobi -> Çöpe Taşı
                        {
                            await _cihazServisi.CihazDurumuGuncelleAsync(id, 3);
                             XtraMessageBox.Show("Cihaz çöp kutusuna taşındı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        BtnTemizle_Tikla(null, EventArgs.Empty);
                        await GridiYenile();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show($"İşlem hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void BtnWifiKaydet_Tikla(object? sender, EventArgs e)
        {
            if (!_seriPortServisi.AcikMi) { XtraMessageBox.Show("Seri port bağlı değil!"); return; }
            string ssid = txtSsid.Text;
            string pass = txtPass.Text;
            _seriPortServisi.VeriYaz($"WIFI_SET:{ssid}|{pass}");
            XtraMessageBox.Show("WiFi parametreleri Master cihaza gönderildi.");
        }

        private void BtnArama_Tikla(object? sender, EventArgs e)
        {
            if (!_seriPortServisi.AcikMi) { XtraMessageBox.Show("Seri port bağlı değil!"); return; }
            _seriPortServisi.VeriYaz("NET_SCAN");
            XtraMessageBox.Show("Ağ tarama komutu gönderildi. Cihaz listesi bekleniyor...");
        }

        private async void BtnYapilandirmaYaz_Tikla(object? sender, EventArgs e)
        {
            var cihaz = lookCfgDevice.GetSelectedDataRow() as Cihaz;
            if (cihaz == null) return;

            cihaz.VeriPeriyodu = (int)numCfgPeriod.Value;
            cihaz.SensorSecimi = chkCfgSensors.EditValue?.ToString() ?? "ALL";
            
            await _cihazServisi.IotAyarlariniGuncelleAsync(cihaz);
            
            if (_seriPortServisi.AcikMi)
            {
                _seriPortServisi.VeriYaz($"CFG_SET:{cihaz.Id}|{cihaz.VeriPeriyodu}|{cihaz.SensorSecimi}");
                XtraMessageBox.Show("Yapılandırma veritabanına kaydedildi ve donanıma gönderildi.");
            }
            else
            {
                 XtraMessageBox.Show("Yapılandırma veritabanına kaydedildi (Donanım bağlı değil).");
            }
        }

        private void BtnYapilandirmaTesti_Tikla(object? sender, EventArgs e)
        {
            if (lookCfgDevice.EditValue == null) return;
            if (!_seriPortServisi.AcikMi) { XtraMessageBox.Show("Seri port bağlı değil!"); return; }
            
            _seriPortServisi.VeriYaz($"CMD_PING:{lookCfgDevice.EditValue}");
        }

        private void LookYapilandirmaCihazi_Degisti(object? sender, EventArgs e)
        {
            var cihaz = lookCfgDevice.GetSelectedDataRow() as Cihaz;
            if (cihaz != null)
            {
                numCfgPeriod.Value = cihaz.VeriPeriyodu;
                chkCfgSensors.EditValue = cihaz.SensorSecimi;
            }
        }
    }
}
