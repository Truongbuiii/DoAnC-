using System.Collections.Concurrent;

namespace FoodTourApp.Services
{
    /// <summary>
    /// NarrationService - Quản lý phát thuyết minh (TTS hoặc Audio file)
    /// - Hàng chờ (queue) để không phát chồng lên nhau
    /// - Tránh trùng lặp
    /// - Hỗ trợ hủy đang phát
    /// </summary>
    public class NarrationService
    {
        // Hàng chờ thuyết minh
        private readonly ConcurrentQueue<NarrationItem> _queue = new();
        
        // Token để hủy TTS đang phát
        private CancellationTokenSource? _currentCts;
        
        // Đang phát hay không
        private bool _isPlaying = false;
        
        // Lock để tránh race condition
        private readonly SemaphoreSlim _playLock = new(1, 1);
        
        // ID của nội dung đang phát (tránh phát lại cùng 1 nội dung)
        private string _currentPlayingId = string.Empty;

        // Event khi bắt đầu phát
        public event EventHandler<NarrationEventArgs>? OnNarrationStarted;
        
        // Event khi kết thúc phát
        public event EventHandler<NarrationEventArgs>? OnNarrationCompleted;
        
        // Event khi có lỗi
        public event EventHandler<string>? OnNarrationError;

        /// <summary>
        /// Thêm vào hàng chờ và phát
        /// </summary>
        /// <param name="id">ID duy nhất (vd: POI Id)</param>
        /// <param name="text">Nội dung cần đọc</param>
        /// <param name="priority">Độ ưu tiên (cao hơn = phát trước)</param>
        public async Task QueueAndPlayAsync(string id, string text, int priority = 0)
        {
            // Tránh phát lại cùng nội dung đang phát
            if (_currentPlayingId == id && _isPlaying)
                return;

            var item = new NarrationItem
            {
                Id = id,
                Text = text,
                Priority = priority,
                Type = NarrationType.TTS
            };

            _queue.Enqueue(item);
            await ProcessQueueAsync();
        }

        /// <summary>
        /// Phát ngay lập tức (hủy cái đang phát nếu có)
        /// </summary>
        public async Task PlayImmediateAsync(string id, string text)
        {
            // Hủy cái đang phát
            await StopAsync();
            
            // Xóa hàng chờ
            while (_queue.TryDequeue(out _)) { }
            
            // Phát ngay
            await PlayTtsAsync(id, text);
        }

        /// <summary>
        /// Phát lại nội dung cụ thể (dùng cho nút Replay)
        /// </summary>
        public async Task ReplayAsync(string id, string text)
        {
            // Hủy cái đang phát
            await StopAsync();
            
            // Phát nội dung mới
            await PlayTtsAsync(id, text);
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
            // Nếu đang phát thì không xử lý
            if (_isPlaying) return;

            await _playLock.WaitAsync();
            try
            {
                if (_queue.TryDequeue(out var item))
                {
                    if (item.Type == NarrationType.TTS)
                    {
                        await PlayTtsAsync(item.Id, item.Text);
                    }
                    // TODO: Thêm hỗ trợ Audio file sau
                }
            }
            finally
            {
                _playLock.Release();
            }
        }

        /// <summary>
        /// Phát TTS
        /// </summary>
        private async Task PlayTtsAsync(string id, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            _isPlaying = true;
            _currentPlayingId = id;
            _currentCts = new CancellationTokenSource();

            try
            {
                // Thông báo bắt đầu phát
                OnNarrationStarted?.Invoke(this, new NarrationEventArgs { Id = id, Text = text });

                // Cấu hình giọng đọc tiếng Việt
                var options = new SpeechOptions
                {
                    Volume = 1.0f,
                    Pitch = 1.0f
                };

                // Phát TTS
                await TextToSpeech.Default.SpeakAsync(text, options, _currentCts.Token);

                // Thông báo kết thúc
                OnNarrationCompleted?.Invoke(this, new NarrationEventArgs { Id = id, Text = text });
            }
            catch (OperationCanceledException)
            {
                // Bị hủy - bình thường
            }
            catch (Exception ex)
            {
                OnNarrationError?.Invoke(this, ex.Message);
            }
            finally
            {
                _isPlaying = false;
                _currentPlayingId = string.Empty;
                
                // Xử lý item tiếp theo trong hàng chờ
                await ProcessQueueAsync();
            }
        }

        /// <summary>
        /// Kiểm tra đang phát không
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Lấy số lượng trong hàng chờ
        /// </summary>
        public int QueueCount => _queue.Count;

        /// <summary>
        /// Xóa toàn bộ hàng chờ
        /// </summary>
        public void ClearQueue()
        {
            while (_queue.TryDequeue(out _)) { }
        }
    }

    /// <summary>
    /// Loại thuyết minh
    /// </summary>
    public enum NarrationType
    {
        TTS,        // Text-to-Speech
        AudioFile   // File âm thanh có sẵn
    }

    /// <summary>
    /// Item trong hàng chờ
    /// </summary>
    public class NarrationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string? AudioPath { get; set; }
        public NarrationType Type { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Event args cho narration
    /// </summary>
    public class NarrationEventArgs : EventArgs
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
