namespace FoodTourApp.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly string[] _languageCodes = { "vi-VN", "en-US", "zh-CN", "ko-KR", "ja-JP" };

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var lang = Preferences.Get("AppLanguage", "vi-VN");
        LangPicker.SelectedIndex = Array.IndexOf(_languageCodes, lang);
        AutoNarrateSwitch.IsToggled = Preferences.Get("AutoNarrate", true);
        CooldownSlider.Value = Preferences.Get("CooldownMinutes", 5);
        RadiusLabel.Text = $"{(int)RadiusSlider.Value}m";
        CooldownLabel.Text = $"{(int)CooldownSlider.Value} phút";
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LangPicker.SelectedIndex < 0) return;
        Preferences.Set("AppLanguage", _languageCodes[LangPicker.SelectedIndex]);
    }

    private void OnAutoNarrateToggled(object sender, ToggledEventArgs e)
        => Preferences.Set("AutoNarrate", e.Value);

    private void OnRadiusChanged(object sender, ValueChangedEventArgs e)
        => RadiusLabel.Text = $"{(int)e.NewValue}m";

    private void OnCooldownChanged(object sender, ValueChangedEventArgs e)
    {
        int val = (int)e.NewValue;
        CooldownLabel.Text = $"{val} phút";
        Preferences.Set("CooldownMinutes", val);
    }

    private async void OnClearFavorites(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Xác nhận", "Xóa toàn bộ danh sách yêu thích?", "Xóa", "Hủy");
        if (!confirm) return;
        Preferences.Remove("favorites");
        await DisplayAlertAsync("Thành công", "Đã xóa danh sách yêu thích!", "OK");
    }
}