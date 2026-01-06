using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Business;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class SCihaz : ICihazServisi
    {
        private readonly BCihaz _isMantigi;

        public SCihaz(string baglantiCumlesi)
        {
            _isMantigi = new BCihaz(baglantiCumlesi);
        }

        public async Task<List<Cihaz>> TumCihazlariGetirAsync()
        {
            return await _isMantigi.TumCihazlariGetirAsync();
        }

        public async Task CihazEkleAsync(Cihaz cihaz)
        {
            await _isMantigi.CihazEkleAsync(cihaz);
        }

        public async Task CihazGuncelleAsync(Cihaz cihaz)
        {
            await _isMantigi.CihazGuncelleAsync(cihaz);
        }

        public async Task CihazSilAsync(int id)
        {
            await _isMantigi.CihazSilAsync(id);
        }
        public async Task CihazDurumuGuncelleAsync(int id, int durum)
        {
            await _isMantigi.CihazDurumuGuncelleAsync(id, durum);
        }

        public async Task IotAyarlariniGuncelleAsync(Cihaz cihaz)
        {
            await _isMantigi.IotAyarlariniGuncelleAsync(cihaz);
        }
    }
}
