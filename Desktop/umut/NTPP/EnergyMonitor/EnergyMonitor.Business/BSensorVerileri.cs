using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.SP;

namespace EnergyMonitor.Business
{
    public class BSensorVerileri
    {
        private readonly SpSensorVerileri _spSensorVerileri;

        public BSensorVerileri(string baglantiCumlesi)
        {
            _spSensorVerileri = new SpSensorVerileri(baglantiCumlesi);
        }

        public async Task VeriEkleAsync(int cihazId, string sensorTuru, decimal? deger, string? birim, string? telemetriJson = null)
        {
            await _spSensorVerileri.VeriEkleAsync(cihazId, sensorTuru, deger, birim, telemetriJson);
        }

        public async Task<List<SensorVerisi>> VerileriGetirAsync(List<int> cihazIdleri, System.DateTime baslangic, System.DateTime bitis)
        {
            return await _spSensorVerileri.VerileriGetirAsync(cihazIdleri, baslangic, bitis);
        }
    }
}
