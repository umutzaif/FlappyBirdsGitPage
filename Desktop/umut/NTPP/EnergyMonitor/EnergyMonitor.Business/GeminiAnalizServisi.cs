using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace EnergyMonitor.Business
{
    public interface IGeminiAnalizServisi
    {
        string ApiAnahtari { get; }
        Task<(string Tavsiye, decimal TahminiFatura)> EnerjiKullaniminiAnalizEtAsync(decimal toplamKwh, int gunSayisi, decimal birimMaliyet, decimal vergiOrani = 18.0m, string baglamVerisi = "");
    }

    public class GeminiAnalizServisi : IGeminiAnalizServisi
    {
        private readonly HttpClient _httpIstemcisi;
        public string ApiAnahtari { get; private set; }
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiAnalizServisi(string apiAnahtari)
        {
            ApiAnahtari = apiAnahtari?.Trim() ?? string.Empty;
            
            var isleyici = new HttpClientHandler
            {
                UseProxy = true,
                DefaultProxyCredentials = System.Net.CredentialCache.DefaultCredentials
            };
            
            _httpIstemcisi = new HttpClient(isleyici);
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
        }

        public async Task<(string Tavsiye, decimal TahminiFatura)> EnerjiKullaniminiAnalizEtAsync(decimal toplamKwh, int gunSayisi, decimal birimMaliyet, decimal vergiOrani = 18.0m, string baglamVerisi = "")
        {
            var taslakMesaj = $@"
Role: Endüstriyel Enerji Verimliliği Uzmanı.
Context: Aşağıdaki SCADA verilerine dayanarak bir İŞLETME/FABRİKA için enerji analizi yapıyorsun.
DİKKAT: Ev tipi tavsiyeler (TV fişini çek, LED tak) VERME. Sadece operasyonel, makine bazlı ve sistem optimizasyonu odaklı teknik tavsiyeler ver.

Girdi Verileri:
- Toplam Tüketim: {toplamKwh:F2} kWh ({gunSayisi} gün)
- Birim Ücret: {birimMaliyet:F2} TL/kWh
- Vergi Oranı: %{vergiOrani:F2}
- Sistem Logları/Cihaz Durumları:
{baglamVerisi}

Görev 1: Belirtilen tüketim, birim ücret ve vergileri kullanarak bir sonraki ay sonu (30 günlük) toplam fatura tutarını TAHMİN ET (Vergiler dahil).
Görev 2: Loglara bakarak işletme sahibine yönelik 3 adet spesifik, TEKNİK ve UYGULANABİLİR tavsiye ver. 
DİL: Çıktılar tamamen TÜRKÇE olmalıdır.

Sadece geçerli JSON formatında çıktı ver:
{{
  ""estimated_bill"": <sayi>,
  ""advice"": ""<Turkce_tavsiye_metni>""
}}
";
            var istekGovdesi = new
            {
                contents = new[] { new { parts = new[] { new { text = taslakMesaj } } } }
            };
            var jsonIcerik = new StringContent(JsonSerializer.Serialize(istekGovdesi), Encoding.UTF8, "application/json");

            string? secilenUrl = null;
            var hataLogu = new StringBuilder();

            try 
            {
                using(var listeYaniti = await _httpIstemcisi.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={ApiAnahtari}"))
                {
                    if(listeYaniti.IsSuccessStatusCode)
                    {
                        var jsonModeller = await listeYaniti.Content.ReadAsStringAsync();
                        using var modellerDokumani = JsonDocument.Parse(jsonModeller);
                        if(modellerDokumani.RootElement.TryGetProperty("models", out var modellerDizisi))
                        {
                            var tercihler = new[] { "gemini-2.0-flash", "gemini-2.0-flash-001", "gemini-flash-latest", "gemini-2.5-flash", "gemini-1.5-flash" };

                            foreach(var m in modellerDizisi.EnumerateArray())
                            {
                                var ad = m.GetProperty("name").GetString();
                                foreach(var tercih in tercihler)
                                {
                                    if(ad != null && (ad.EndsWith(tercih) || ad.Contains(tercih)))
                                    {
                                        if(m.TryGetProperty("supportedGenerationMethods", out var metodlar))
                                        {
                                            if(metodlar.EnumerateArray().Any(x => x.GetString() == "generateContent"))
                                            {
                                                secilenUrl = $"https://generativelanguage.googleapis.com/v1beta/{ad}:generateContent";
                                                goto KesifTamamlandi; 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    KesifTamamlandi:;
                }
            }
            catch(Exception ex) { hataLogu.AppendLine($"[Discovery] hata: {ex.Message}"); }

            var uclar = new List<string>();
            if(secilenUrl != null) uclar.Add(secilenUrl);
            
            uclar.Add("https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent");
            uclar.Add("https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent");

            foreach (var temelUrl in uclar.Distinct())
            {
                try
                {
                    using (var yanit = await _httpIstemcisi.PostAsync($"{temelUrl}?key={ApiAnahtari}", jsonIcerik))
                    {
                        if (yanit.IsSuccessStatusCode)
                        {
                            var jsonYaniti = await yanit.Content.ReadAsStringAsync();
                            using var dokuman = JsonDocument.Parse(jsonYaniti);
                            var metin = dokuman.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                            if (metin == null) continue;
                            var temizJson = metin.Replace("```json", "").Replace("```", "").Trim();
                            using var sonucDokumani = JsonDocument.Parse(temizJson);
                            
                            var fatura = sonucDokumani.RootElement.GetProperty("estimated_bill").GetDecimal();
                            var tavsiye = sonucDokumani.RootElement.GetProperty("advice").GetString();
                            return (tavsiye ?? string.Empty, fatura);
                        }
                        else
                        {
                            var govde = await yanit.Content.ReadAsStringAsync();
                            hataLogu.AppendLine($"[{temelUrl}] {(int)yanit.StatusCode}: {govde}");
                        }
                    }
                }
                catch (Exception ex) { hataLogu.AppendLine($"[{temelUrl}] hata: {ex.Message}"); }
            }
            
            return (CevrimdisiTavsiyeOlustur(toplamKwh, gunSayisi, birimMaliyet, hataLogu.ToString()), toplamKwh * birimMaliyet);
        }

        private string CevrimdisiTavsiyeOlustur(decimal toplamKwh, int gunSayisi, decimal maliyet, string hata)
        {
            decimal tahmini = toplamKwh * maliyet;
            return $@"⚠️ [ÇEVRİMDIŞI MOD - ANALİZ HATASI]
Hata: {hata}
Tahmini Fatura: {tahmini:F2} TL
Lütfen internet bağlantınızı ve API anahtarınızı kontrol edin.";
        }
    }
}
