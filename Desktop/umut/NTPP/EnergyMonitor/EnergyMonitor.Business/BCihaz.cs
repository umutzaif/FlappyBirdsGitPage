using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.SP;

namespace EnergyMonitor.Business
{
    public class BCihaz
    {
        private readonly SpCihaz _spCihaz;

        public BCihaz(string baglantiCumlesi)
        {
            _spCihaz = new SpCihaz(baglantiCumlesi);
        }

        public async Task<List<Cihaz>> TumCihazlariGetirAsync()
        {
            return await _spCihaz.TumCihazlariGetirAsync();
        }

        public async Task CihazEkleAsync(Cihaz cihaz)
        {
            // İş Kuralı: İsim zorunludur
            if (string.IsNullOrWhiteSpace(cihaz.Ad))
                throw new System.ArgumentException("Cihaz Adı boş olamaz.");

            await _spCihaz.CihazEkleAsync(cihaz);
        }

        public async Task CihazGuncelleAsync(Cihaz cihaz)
        {
             if (cihaz.Id <= 0)
                throw new System.ArgumentException("Geçersiz Cihaz ID.");

            await _spCihaz.CihazGuncelleAsync(cihaz);
        }

        public async Task CihazSilAsync(int id)
        {
            if (id <= 0)
                 throw new System.ArgumentException("Geçersiz Cihaz ID.");
                 
            await _spCihaz.CihazSilAsync(id);
        }
        public async Task CihazDurumuGuncelleAsync(int id, int durum)
        {
             if (id <= 0) throw new System.ArgumentException("Geçersiz Cihaz ID.");
             await _spCihaz.CihazDurumuGuncelleAsync(id, durum);
        }

        public async Task IotAyarlariniGuncelleAsync(Cihaz cihaz)
        {
            if (cihaz.Id <= 0) throw new System.ArgumentException("Geçersiz Cihaz ID.");
            await _spCihaz.IotAyarlariniGuncelleAsync(cihaz);
        }
    }
}
