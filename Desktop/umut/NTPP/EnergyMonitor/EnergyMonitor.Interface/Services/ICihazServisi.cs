using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Interface.Services
{
    public interface ICihazServisi
    {
        Task<List<Cihaz>> TumCihazlariGetirAsync();
        Task CihazEkleAsync(Cihaz cihaz);
        Task CihazGuncelleAsync(Cihaz cihaz);
        Task CihazDurumuGuncelleAsync(int id, int durum);
        Task CihazSilAsync(int id);
        Task IotAyarlariniGuncelleAsync(Cihaz cihaz);
    }
}
