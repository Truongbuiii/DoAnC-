using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Maui.Devices.Sensors;

namespace FoodTourApp.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public class LocationForegroundService : Service
{
    private const int NotificationId = 1001;
    private const string ChannelId = "location_channel";
    private CancellationTokenSource? _cts;

    // Event để MapPage nhận tọa độ
    public static event EventHandler<Location>? LocationUpdated;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();
        var notification = BuildNotification();
        StartForeground(NotificationId, notification,
            global::Android.Content.PM.ForegroundService.TypeLocation);

        _cts = new CancellationTokenSource();
        Task.Run(() => TrackLocationAsync(_cts.Token));

        return StartCommandResult.Sticky;
    }

    private async Task TrackLocationAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request, token);
                if (location != null)
                    LocationUpdated?.Invoke(this, location);
            }
            catch { }

            await Task.Delay(15000, token); // cập nhật mỗi 15 giây
        }
    }

    public override void OnDestroy()
    {
        _cts?.Cancel();
        base.OnDestroy();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var channel = new NotificationChannel(
            ChannelId,
            "Vĩnh Khánh Tour Location",
            NotificationImportance.Low)
        {
            Description = "Theo dõi vị trí để phát thuyết minh tự động"
        };

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification()
    {
        var intent = new Intent(this, typeof(global::FoodTourApp.MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        return new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("🗺️ Vĩnh Khánh Tour")
            .SetContentText("Đang theo dõi vị trí để phát thuyết minh...")
            .SetSmallIcon(Resource.Drawable.abc_ic_menu_copy_mtrl_am_alpha)
            .SetContentIntent(pendingIntent)
            .SetOngoing(true)
            .Build();
    }
}