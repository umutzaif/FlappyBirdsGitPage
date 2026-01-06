using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyMonitor.Business;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.Interface.Services;

namespace EnergyMonitor.Service
{
    public class SKullanici : IKullaniciServisi
    {
        private readonly BKullanici _isMantigi;

        public SKullanici(string baglantiCumlesi)
        {
            _isMantigi = new BKullanici(baglantiCumlesi);
        }

        public async Task<Kullanici?> GirisYapAsync(string kullaniciAdi, string sifre)
        {
            // Note: In a real app, this might call a separate auth business logic
            // For this project, we'll keep it simple and route through SpKullanici via Business
            return await _isMantigi.GirisYapAsync(kullaniciAdi, sifre);
        }

        public async Task<bool> KayitOlAsync(Kullanici kullanici)
        {
            await _isMantigi.KullaniciEkleAsync(kullanici);
            return true;
        }

        public async Task<List<Kullanici>> IsletmeyeGoreKullanicilariGetirAsync(int isletmeId)
        {
            return await _isMantigi.IsletmeyeGoreKullanicilariGetirAsync(isletmeId);
        }

        public async Task<bool> KullaniciDurumuGuncelleAsync(int kullaniciId, bool aktifMi)
        {
            await _isMantigi.KullaniciDurumuGuncelleAsync(kullaniciId, aktifMi);
            return true;
        }

        public async Task<bool> SifreSifirlaAsync(int kullaniciId, string yeniSifre)
        {
            return await _isMantigi.SifreSifirlaAsync(kullaniciId, yeniSifre);
        }

        public async Task<IEnumerable<Kullanici>> TumKullanicilariGetirAsync()
        {
            return await _isMantigi.TumKullanicilariGetirAsync();
        }

        public async Task<IEnumerable<Rol>> TumRolleriGetirAsync()
        {
            return await _isMantigi.TumRolleriGetirAsync();
        }

        public async Task KullaniciEkleAsync(Kullanici kullanici)
        {
            await _isMantigi.KullaniciEkleAsync(kullanici);
        }

        public async Task KullaniciGuncelleAsync(Kullanici kullanici)
        {
            await _isMantigi.KullaniciGuncelleAsync(kullanici);
        }

        public async Task KullaniciSilAsync(int id)
        {
            await _isMantigi.KullaniciSilAsync(id);
        }

        public async Task SemayiHazirlaAsync()
        {
            await _isMantigi.SemayiHazirlaAsync();
        }
    }
}
