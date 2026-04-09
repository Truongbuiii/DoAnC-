using System.Collections.Concurrent;

namespace FoodTourApp.Services
{
    /// <summary>
    /// NarrationService - Quản lý phát thuyết minh (TTS hoặc Audio file)
    /// - Hàng chờ (queue) để không phát chồng lên nhau
    /// - Tránh trùng lặp
    /// - Hỗ trợ đa ngôn ngữ (5 ngôn ngữ)
    /// - Hỗ trợ hủy đang phát
    /// </summary>
    public class NarrationService
    {
        private readonly Dictionary<string, DateTime> _lastPlayedTimes = new();
        // Hàng chờ thuyết minh
        private readonly ConcurrentQueue<NarrationItem> _queue = new();

        // Token để hủy TTS đang phát
        private CancellationTokenSource? _currentCts;

        // Đang phát hay không
        private bool _isPlaying = false;

        // Lock để tránh race condition
        private readonly SemaphoreSlim _playLock = new(1, 1);

        // ID của nội dung đang phát
        private string _currentPlayingId = string.Empty;

        // ============================================================
        // ĐA NGÔN NGỮ: Ngôn ngữ hiện tại
        // ============================================================
        private string _currentLanguage = "vi-VN";

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set => _currentLanguage = value;
        }

        // Event khi bắt đầu phát
        public event EventHandler<NarrationEventArgs>? OnNarrationStarted;

        // Event khi kết thúc phát
        public event EventHandler<NarrationEventArgs>? OnNarrationCompleted;

        // Event khi có lỗi
        public event EventHandler<string>? OnNarrationError;

        /// <summary>
        /// Thêm vào hàng chờ và phát
        /// </summary>
        public async Task QueueAndPlayAsync(string id, string text, bool isUrgent = false)
        {
            // 1. Chống trùng lặp (Sửa lỗi CS0103)
            if (_lastPlayedTimes.TryGetValue(id, out var lastTime) &&
                (DateTime.Now - lastTime).TotalSeconds < 30)
                return;

            if (isUrgent)
            {
                await PlayImmediateAsync(id, text);
                _lastPlayedTimes[id] = DateTime.Now;
                return;
            }

            // 2. Sửa lỗi CS8635 & CS0747 (Thay dấu ... bằng code thật)
            var item = new NarrationItem
            {
                Id = id,
                Text = text,
                Priority = isUrgent ? 10 : 0, // Gán giá trị thật thay vì để ...
                Type = NarrationType.TTS,
                LanguageCode = _currentLanguage
            };

            _queue.Enqueue(item);
            _lastPlayedTimes[id] = DateTime.Now; // Lưu lại vết phát
            await ProcessQueueAsync();
        }

        /// <summary>
        /// Phát ngay lập tức (hủy cái đang phát nếu có)
        /// </summary>
        public async Task PlayImmediateAsync(string id, string text)
        {
            await StopAsync();
            while (_queue.TryDequeue(out _)) { }
            await PlayTtsAsync(id, text, _currentLanguage);
        }

        /// <summary>
        /// Phát lại nội dung cụ thể
        /// </summary>
        public async Task ReplayAsync(string id, string text)
        {
            await StopAsync();
            await PlayTtsAsync(id, text, _currentLanguage);
        }

        /// <summary>
        /// Dừng phát hiện tại
        /// </summary>
        public async Task StopAsync()
        {
            try
            {
                _currentCts?.Cancel();
                _isPlaying = false;
                _currentPlayingId = string.Empty;
            }
            catch { }
        }

        /// <summary>
        /// Xử lý hàng chờ
        /// </summary>
        private async Task ProcessQueueAsync()
        {
            if (_isPlaying) return;

            await _playLock.WaitAsync();
            try
            {
                if (_queue.TryDequeue(out var item))
                {
                    if (item.Type == NarrationType.TTS)
                    {
                        await PlayTtsAsync(item.Id, item.Text, item.LanguageCode);
                    }
                }
            }
            finally
            {
                _playLock.Release();
            }
        }

        /// <summary>
        /// Phát TTS với ngôn ngữ cụ thể
        /// </summary>
        private async Task PlayTtsAsync(string id, string text, string languageCode)
        {
            // DEBUG: Kiểm tra text có rỗng không
            if (string.IsNullOrEmpty(text))
            {
                OnNarrationError?.Invoke(this, "Text rỗng!");
                return;
            }

            _isPlaying = true;
            _currentPlayingId = id;
            _currentCts = new CancellationTokenSource();

            try
            {
                OnNarrationStarted?.Invoke(this, new NarrationEventArgs
                {
                    Id = id,
                    Text = text,
                    LanguageCode = languageCode
                });

                // DEBUG: Thử phát không cần locale trước
                await TextToSpeech.Default.SpeakAsync(text, cancelToken: _currentCts.Token);

                OnNarrationCompleted?.Invoke(this, new NarrationEventArgs
                {
                    Id = id,
                    Text = text,
                    LanguageCode = languageCode
                });
            }
            catch (Exception ex)
            {
                // DEBUG: Hiện lỗi
                OnNarrationError?.Invoke(this, $"Lỗi TTS: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
                _currentPlayingId = string.Empty;
                await ProcessQueueAsync();
            }
        }


        /// <summary>
        /// Tìm locale phù hợp nhất từ danh sách có sẵn
        /// </summary>
        private Microsoft.Maui.Media.Locale? FindBestLocale(
            IEnumerable<Microsoft.Maui.Media.Locale> locales,
            string languageCode)
        {
            // Tách language và country từ code (vd: "vi-VN" -> "vi", "VN")
            var parts = languageCode.Split('-');
            var language = parts[0].ToLower();
            var country = parts.Length > 1 ? parts[1].ToUpper() : "";

            // Tìm exact match (vd: vi-VN)
            var exactMatch = locales.FirstOrDefault(l =>
                l.Language?.ToLower() == language &&
                l.Country?.ToUpper() == country);

            if (exactMatch != null) return exactMatch;

            // Tìm language match (vd: vi)
            var langMatch = locales.FirstOrDefault(l =>
                l.Language?.ToLower() == language);

            if (langMatch != null) return langMatch;

            // Trả về null để dùng giọng mặc định
            return null;
        }

        /// <summary>
        /// Lấy danh sách ngôn ngữ TTS có sẵn trên thiết bị
        /// </summary>
        public async Task<List<string>> GetAvailableLanguagesAsync()
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            return locales
                .Select(l => $"{l.Language}-{l.Country}")
                .Distinct()
                .ToList();
        }

        public bool IsPlaying => _isPlaying;
        public int QueueCount => _queue.Count;

        public void ClearQueue()
        {
            while (_queue.TryDequeue(out _)) { }
        }
    }

    public enum NarrationType
    {
        TTS,
        AudioFile
    }

    public class NarrationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? AudioPath { get; set; }
        public NarrationType Type { get; set; }
        public int Priority { get; set; }
        public string LanguageCode { get; set; } = "vi-VN";
    }

    public class NarrationEventArgs : EventArgs
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "vi-VN";
    }
}