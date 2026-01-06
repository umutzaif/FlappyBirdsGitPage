using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Interface.Services
{
    public interface IIsletmeServisi
    {
        Task<IsletmeBilgisi?> ProfilGetirAsync(int isletmeId);
        Task<bool> ProfilKaydetAsync(IsletmeBilgisi profil);
    }
}
