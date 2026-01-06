using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnergyMonitor.Interface.Models;
using EnergyMonitor.SP;

namespace EnergyMonitor.Business
{
    public class BKullanici
    {
        private readonly SpKullanici _spKullanici;

        public BKullanici(string baglantiCumlesi)
        {
            _spKullanici = new SpKullanici(baglantiCumlesi);
        }

        public async Task<IEnumerable<Kullanici>> TumKullanicilariGetirAsync()
        {
            return await _spKullanici.TumKullanicilariGetirAsync();
        }

        public async Task<IEnumerable<Rol>> TumRolleriGetirAsync()
        {
            return await _spKullanici.TumRolleriGetirAsync();
        }

        public async Task KullaniciEkleAsync(Kullanici kullanici)
        {
            // İş Mantığı: Şifreleme eklenebilir.
            await _spKullanici.KullaniciEkleAsync(kullanici);
        }

        public async Task KullaniciGuncelleAsync(Kullanici kullanici)
        {
            await _spKullanici.KullaniciGuncelleAsync(kullanici);
        }

        public async Task KullaniciSilAsync(int id)
        {
            await _spKullanici.KullaniciSilAsync(id);
        }

        public async Task<Kullanici?> GirisYapAsync(string kullaniciAdi, string sifre)
        {
            return await _spKullanici.GirisYapAsync(kullaniciAdi, sifre);
        }

        public async Task<List<Kullanici>> IsletmeyeGoreKullanicilariGetirAsync(int isletmeId)
        {
            // Placeholder: SpKullanici might need this method. Checking soon.
            var all = await _spKullanici.TumKullanicilariGetirAsync();
            return all.Where(u => u.IsletmeId == isletmeId).ToList();
        }

        public async Task<bool> KullaniciDurumuGuncelleAsync(int kullaniciId, bool aktifMi)
        {
            // Placeholder: SpKullanici might need specific update.
            // For now, get, update, save. (Or add to SP)
            var users = await _spKullanici.TumKullanicilariGetirAsync();
            var user = users.FirstOrDefault(u => u.Id == kullaniciId);
            if (user != null)
            {
                user.AktifMi = aktifMi;
                await _spKullanici.KullaniciGuncelleAsync(user);
                return true;
            }
            return false;
        }

        public async Task<bool> SifreSifirlaAsync(int kullaniciId, string yeniSifre)
        {
            var users = await _spKullanici.TumKullanicilariGetirAsync();
            var user = users.FirstOrDefault(u => u.Id == kullaniciId);
            if (user != null)
            {
                user.SifreOzeti = yeniSifre; // Simple set for now
                await _spKullanici.KullaniciGuncelleAsync(user);
                return true;
            }
            return false;
        }

        public async Task SemayiHazirlaAsync()
        {
            await _spKullanici.SemayiHazirlaAsync();
        }
    }
}
