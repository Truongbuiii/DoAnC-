using Android.Speech.Tts;
using AndroidTts = Android.Speech.Tts.TextToSpeech;

namespace FoodTourApp.Platforms.Android
{
    public class AndroidTtsService : Java.Lang.Object, AndroidTts.IOnInitListener
    {
        private AndroidTts? _tts;
        private bool _isInitialized = false;
        private TaskCompletionSource<bool>? _initTcs;

        public async Task<bool> InitializeAsync()
        {
            // Thử khởi tạo 3 lần
            for (int i = 0; i < 3; i++)
            {
                _initTcs = new TaskCompletionSource<bool>();
                _tts = new AndroidTts(global::Android.App.Application.Context, this);

                // Đợi tối đa 5 giây
                var timeoutTask = Task.Delay(5000);
                var completedTask = await Task.WhenAny(_initTcs.Task, timeoutTask);

                if (completedTask == _initTcs.Task && _isInitialized)
                {
                    return true;
                }

                // Đợi 1 giây rồi thử lại
                await Task.Delay(1000);
            }

            return false;
        }

        public void OnInit(OperationResult status)
        {
            _isInitialized = (status == OperationResult.Success);

            if (_isInitialized && _tts != null)
            {
                _tts.SetLanguage(Java.Util.Locale.Default);
            }

            _initTcs?.TrySetResult(_isInitialized);
        }

        public void SetLanguage(string languageCode)
        {
            if (_tts == null) return;

            var locale = languageCode switch
            {
                "vi-VN" => new Java.Util.Locale("vi", "VN"),
                "en-US" => new Java.Util.Locale("en", "US"),
                "zh-CN" => new Java.Util.Locale("zh", "CN"),
                "ko-KR" => new Java.Util.Locale("ko", "KR"),
                "ja-JP" => new Java.Util.Locale("ja", "JP"),
                _ => Java.Util.Locale.Default
            };

            _tts.SetLanguage(locale);
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