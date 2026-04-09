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
                // KHAI BÁO BIẾN CHO KHỚP VỚI URL
                string from = "vi"; // Ngôn ngữ nguồn luôn là tiếng Việt
                string to = targetLang; // Ngôn ngữ đích truyền từ ngoài vào (en, ja, ko...)
                string myEmail = "buiductruong0001@gmail.com";

                // Sửa URL: Truyền đúng biến from và to vào
                string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={from}|{to}&de={myEmail}";

                var response = await _httpClient.GetStringAsync(url);
                Debug.WriteLine($"=== API RESPONSE: {response}");

                using (var doc = JsonDocument.Parse(response))
                {
                    var root = doc.RootElement;
                    var responseData = root.GetProperty("responseData");
                    var translatedText = responseData.GetProperty("translatedText").GetString();

                    // Kiểm tra lỗi từ API
                    if (string.IsNullOrEmpty(translatedText) || translatedText.Contains("MYMEMORY WARNING"))
                    {
                        return null;
                    }

                    Debug.WriteLine($"=== TRANSLATED ({targetLang}): {translatedText}");
                    return translatedText;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"=== TRANSLATE FAIL ({targetLang}): {ex.Message}");
                return null;
            }
        }

        // Hàm này dùng để dịch toàn bộ 4 thứ tiếng cho 1 quán
        public async Task TranslatePoiAsync(POI poi)
        {
            if (string.IsNullOrEmpty(poi.DescriptionVi)) return;

            // Dịch lần lượt và nghỉ 1.5s để tránh lỗi 429 (Too Many Requests)
            poi.DescriptionEn = await TranslateAsync(poi.DescriptionVi, "en");
            await Task.Delay(1500);

            poi.DescriptionZh = await TranslateAsync(poi.DescriptionVi, "zh");
            await Task.Delay(1500);

            poi.DescriptionKo = await TranslateAsync(poi.DescriptionVi, "ko");
            await Task.Delay(1500);

            poi.DescriptionJa = await TranslateAsync(poi.DescriptionVi, "ja");
        }
    }
}