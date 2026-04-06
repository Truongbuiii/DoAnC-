using FoodTourApp.Services;

namespace FoodTourApp.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly string[] _languageCodes = { "vi-VN", "en-US", "zh-CN", "ko-KR", "ja-JP" };

    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Lang.Load();
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        Title = Lang.Get("tab_settings");
        LblSecLanguage.Text = Lang.Get("settings_language");
        LblLangLabel.Text = Lang.Get("settings_lang_label");
        LblSecNarration.Text = Lang.Get("settings_narration");
        LblAutoPlay.Text = Lang.Get("settings_auto");
        LblAutoPlaySub.Text = Lang.Get("settings_auto_sub");
        LblSecGps.Text = Lang.Get("settings_gps");
        LblRadius.Text = Lang.Get("settings_radius");
        LblCooldown.Text = Lang.Get("settings_cooldown");
        LblSecData.Text = Lang.Get("settings_data");
        LblClearFav.Text = Lang.Get("settings_clear");
        LblSecInfo.Text = Lang.Get("settings_info");
        LblVersion.Text = Lang.Get("settings_version");
        LblPlatform.Text = Lang.Get("settings_platform");
        CooldownLabel.Text = $"{(int)CooldownSlider.Value} {(Preferences.Get("AppLanguage", "vi-VN") == "vi-VN" ? "phút" : "min")}";
    }

    private void LoadSettings()
    {
        var lang = Preferences.Get("AppLanguage", "vi-VN");
        LangPicker.SelectedIndex = Array.IndexOf(_languageCodes, lang);
        AutoNarrateSwitch.IsToggled = Preferences.Get("AutoNarrate", true);
        CooldownSlider.Value = Preferences.Get("CooldownMinutes", 5);
        RadiusLabel.Text = $"{(int)RadiusSlider.Value}m";
        CooldownLabel.Text = $"{(int)CooldownSlider.Value}";
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LangPicker.SelectedIndex < 0) return;
        var code = _languageCodes[LangPicker.SelectedIndex];
        Preferences.Set("AppLanguage", code);
        Lang.Set(code);
        ApplyLanguage();

        // Cập nhật tab titles ngay lập tức
        if (Shell.Current is AppShell appShell)
            appShell.ApplyLanguage();
    }

    private void OnAutoNarrateToggled(object sender, ToggledEventArgs e)
        => Preferences.Set("AutoNarrate", e.Value);

    private void OnRadiusChanged(object sender, ValueChangedEventArgs e)
    {
        int val = (int)e.NewValue;
        RadiusLabel.Text = $"{val}m";
        Preferences.Set("TriggerRadius", val);
    }

    private void OnCooldownChanged(object sender, ValueChangedEventArgs e)
    {
        int val = (int)e.NewValue;
        CooldownLabel.Text = $"{val}";
        Preferences.Set("CooldownMinutes", val);
    }

    private async void OnClearFavorites(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            Lang.Get("btn_ok"),
            Lang.Get("confirm_clear_fav"),
            Lang.Get("btn_delete"),
            Lang.Get("btn_cancel"));
        if (!confirm) return;
        Preferences.Remove("favorites");
        await DisplayAlertAsync(Lang.Get("btn_ok"), Lang.Get("success_clear_fav"), Lang.Get("btn_ok"));
    }
}