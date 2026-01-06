using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;

namespace EnergyMonitor.Interface.Services
{
    public interface IKullaniciServisi
    {
        Task<Kullanici?> GirisYapAsync(string kullaniciAdi, string sifre);
        Task<bool> KayitOlAsync(Kullanici kullanici);
        Task<List<Kullanici>> IsletmeyeGoreKullanicilariGetirAsync(int isletmeId);
        Task<bool> KullaniciDurumuGuncelleAsync(int kullaniciId, bool aktifMi);
        Task<bool> SifreSifirlaAsync(int kullaniciId, string yeniSifre);
        Task<IEnumerable<Kullanici>> TumKullanicilariGetirAsync();
        Task<IEnumerable<Rol>> TumRolleriGetirAsync();
        Task KullaniciEkleAsync(Kullanici kullanici);
        Task KullaniciGuncelleAsync(Kullanici kullanici);
        Task KullaniciSilAsync(int id);
        Task SemayiHazirlaAsync();
    }
}
