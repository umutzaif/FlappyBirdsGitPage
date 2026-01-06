using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Interface.Services
{
    public interface IMesajServisi
    {
        Task<List<Mesaj>> MesajlariGetirAsync(int kullaniciId);
        Task<bool> MesajGonderAsync(Mesaj mesaj);
        Task OkunduOlarakIsaretleAsync(int mesajId);
        Task MesajSilAsync(int mesajId);
    }
}
