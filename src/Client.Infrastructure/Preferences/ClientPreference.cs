﻿using ZANECO.WASM.Client.Infrastructure.Theme;

namespace ZANECO.WASM.Client.Infrastructure.Preferences;

public class ClientPreference : IPreference
{
    public bool IsDarkMode { get; set; }
    public bool IsRTL { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string PrimaryColor { get; set; } = CustomColors.Light.Primary;
    public string SecondaryColor { get; set; } = CustomColors.Light.Secondary;
    public double BorderRadius { get; set; } = 5;
    public int Elevation { get; set; } = 5;
    public string LanguageCode { get; set; } = LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? "en-US";
    public FshTablePreference TablePreference { get; set; } = new();
    public BackgroundPreference BackgroundPreference { get; set; } = new();
}