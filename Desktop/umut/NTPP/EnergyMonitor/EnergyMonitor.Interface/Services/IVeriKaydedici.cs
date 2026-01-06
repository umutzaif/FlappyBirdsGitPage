using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Interface.Services
{
    public interface IVeriKaydedici
    {
        Task VeriKaydetAsync(SensorVerisi veri);
        Task<List<SensorVerisi>> VerileriGetirAsync(int? cihazId, DateTime baslangic, DateTime bitis);
    }
}
