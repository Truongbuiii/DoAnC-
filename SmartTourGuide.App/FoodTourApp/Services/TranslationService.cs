using System.Text.Json;
using FoodTourApp.Models;
using System.Diagnostics;

namespace FoodTourApp.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;

        public TranslationService()
        {
            // Thêm User-Agent để API không coi mình là bot
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        public async Task<string?> TranslateAsync(string text, string targetLang)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            try
            {
                string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=vi|{targetLang}&de=buiductruong0001@gmail.com";
                var response = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var translatedText = doc.RootElement
                    .GetProperty("responseData")
                    .GetProperty("translatedText")
                    .GetString();

                if (string.IsNullOrEmpty(translatedText) || translatedText.Contains("MYMEMORY WARNING"))
                    return null;

                Debug.WriteLine($"=== TRANSLATED ({targetLang}): {translatedText}");
                return translatedText;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== TRANSLATE FAIL ({targetLang}): {ex.Message}");
                return null;
            }
        }

        // Dịch nhiều ngôn ngữ cho 1 POI nhưng KHÔNG gán vào model.
        // Trả về dictionary mapping mã ngôn ngữ -> chuỗi đã dịch (hoặc null nếu thất bại).
        public async Task<Dictionary<string, string?>> TranslatePoiAsync(POI poi)
        {
            var translations = new Dictionary<string, string?>();
            if (poi == null || string.IsNullOrEmpty(poi.DescriptionVi)) return translations;

            // Dịch lần lượt và nghỉ 1.5s để tránh lỗi 429 (Too Many Requests)
            translations["en"] = await TranslateAsync(poi.DescriptionVi, "en");
            await Task.Delay(1500);

            translations["zh"] = await TranslateAsync(poi.DescriptionVi, "zh");
            await Task.Delay(1500);

            translations["ko"] = await TranslateAsync(poi.DescriptionVi, "ko");
            await Task.Delay(1500);

            translations["ja"] = await TranslateAsync(poi.DescriptionVi, "ja");

            return translations;
        }
    }
}