using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Business;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class SVeriKaydedici : IVeriKaydedici
    {
        private readonly BSensorVerileri _isMantigi;

        public SVeriKaydedici(string baglantiCumlesi)
        {
            _isMantigi = new BSensorVerileri(baglantiCumlesi);
        }

        public async Task VeriKaydetAsync(SensorVerisi veri)
        {
            await _isMantigi.VeriEkleAsync(veri.CihazId, veri.SensorTuru, veri.Deger, veri.Birim, veri.TelemetriVerisi);
        }

        public async Task<List<SensorVerisi>> VerileriGetirAsync(int? cihazId, DateTime baslangic, DateTime bitis)
        {
            var cihazIdleri = cihazId.HasValue ? new List<int> { cihazId.Value } : new List<int>();
            return await _isMantigi.VerileriGetirAsync(cihazIdleri, baslangic, bitis);
        }
    }
}
