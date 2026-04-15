using Android.Speech.Tts;
using AndroidTts = Android.Speech.Tts.TextToSpeech;
using System.Diagnostics;

namespace FoodTourApp.Platforms.Android
{
    public class AndroidTtsService : Java.Lang.Object, AndroidTts.IOnInitListener
    {
        private AndroidTts? _tts;
        private bool _isInitialized = false;
        private TaskCompletionSource<bool>? _initTcs;
        
        // ✅ THÊM: Track ngôn ngữ hiện tại
        private string _currentLanguageCode = "vi-VN";

        public async Task<bool> InitializeAsync()
        {
            for (int i = 0; i < 3; i++)
            {
                _initTcs = new TaskCompletionSource<bool>();
                _tts = new AndroidTts(global::Android.App.Application.Context, this);
                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(_initTcs.Task, timeoutTask);
                if (completedTask == _initTcs.Task && _isInitialized)
                    return true;
                await Task.Delay(1000);
            }
            return false;
        }

        public void OnInit(OperationResult status)
        {
            _isInitialized = (status == OperationResult.Success);
            if (_isInitialized && _tts != null)
            {
                // ✅ SỬA: Set tiếng Việt mặc định thay vì Locale.Default
                var viLocale = new Java.Util.Locale("vi", "VN");
                var result = _tts.SetLanguage(viLocale);
                
                if (result == LanguageAvailableResult.MissingData || 
                    result == LanguageAvailableResult.NotSupported)
                {
                    // Tiếng Việt chưa cài → dùng English làm fallback
                    Debug.WriteLine("=== TTS: Tiếng Việt chưa cài, fallback English");
                    _tts.SetLanguage(Java.Util.Locale.Us);
                }
            }
            _initTcs?.TrySetResult(_isInitialized);
        }

        public void SetLanguage(string languageCode)
        {
            if (_tts == null || !_isInitialized) return;
            
            // ✅ THÊM: Không set lại nếu ngôn ngữ không đổi
            if (_currentLanguageCode == languageCode) return;
            _currentLanguageCode = languageCode;

            var locale = languageCode switch
            {
                "vi-VN" => new Java.Util.Locale("vi", "VN"),
                "en-US" => new Java.Util.Locale("en", "US"),
                "zh-CN" => new Java.Util.Locale("zh", "CN"),
                "ko-KR" => new Java.Util.Locale("ko", "KR"),
                "ja-JP" => new Java.Util.Locale("ja", "JP"),
                _ => Java.Util.Locale.Default
            };

            // ✅ SỬA: Kiểm tra kết quả SetLanguage
            var result = _tts.SetLanguage(locale);
            
            if (result == LanguageAvailableResult.MissingData || 
                result == LanguageAvailableResult.NotSupported)
            {
                Debug.WriteLine($"=== TTS: Ngôn ngữ {languageCode} chưa cài trên máy!");
                
                // Fallback: nếu không có ngôn ngữ yêu cầu → dùng tiếng Anh
                // (tốt hơn là đọc tiếng Việt bằng giọng random)
                _tts.SetLanguage(Java.Util.Locale.Us);
                _currentLanguageCode = "en-US";
            }
            else
            {
                Debug.WriteLine($"=== TTS: Đã set ngôn ngữ {languageCode} ✅");
            }
        }

        public void Speak(string text)
        {
            if (_isInitialized && _tts != null && !string.IsNullOrEmpty(text))
            {
                _tts.Speak(text, QueueMode.Flush, null, null);
            }
        }

        public void Stop()
        {
            _tts?.Stop();
        }

        public bool IsInitialized => _isInitialized;
    }
}