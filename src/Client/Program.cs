using CurrieTechnologies.Razor.SweetAlert2;
//using DevExpress.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;
using System.Globalization;
using ZANECO.WASM.Client;
using ZANECO.WASM.Client.Components.Services;
using ZANECO.WASM.Client.Infrastructure;
using ZANECO.WASM.Client.Infrastructure.Common;
using ZANECO.WASM.Client.Infrastructure.Preferences;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientServices(builder.Configuration);

builder.Services.AddSweetAlert2();

builder.Services.AddSyncfusionBlazor();
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt/QHRqVVhlXFpFdEBBXHxAd1p/VWJYdVt5flBPcDwsT3RfQF5jS39bdkVmUHtdeHNSRA==;Mgo+DSMBPh8sVXJ0S0J+XE9BdlRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TdERrWX9beXBWTmlcUw==;ORg4AjUWIQA/Gnt2VVhkQlFac19JXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkdiUX9edX1RRWlcVEY=;MTE1Mzg3OEAzMjMwMmUzNDJlMzBSUWcxbjJuQmRDQ0s1STB0U1B5SDVodGZBNHlnb1pzK1ZOL2tTR04xZEI4PQ==;MTE1Mzg3OUAzMjMwMmUzNDJlMzBKTmRyVDlhWEVzNjE0LzgyVEEvQmUzQlJLbGlTaCtNVVFmOGNUdUtpS1ZVPQ==;NRAiBiAaIQQuGjN/V0Z+WE9EaFpCVmJLYVB3WmpQdldgdVRMZVVbQX9PIiBoS35RdUVhWHdecHFcQmNVVkZ0;MTE1Mzg4MUAzMjMwMmUzNDJlMzBUTFRFdllBQ1haUEZEeGgwMnZ1WUlQQnlteUtvMHprUWhKVlpYKzBiNFg0PQ==;MTE1Mzg4MkAzMjMwMmUzNDJlMzBpUy9NanM3UFdHZ01sWWp5aEtrWGc4VE1LTmlEd0czOXE5NTlKK2pVRWZnPQ==;Mgo+DSMBMAY9C3t2VVhkQlFac19JXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxQdkdiUX9edX1RQmBfV0M=;MTE1Mzg4NEAzMjMwMmUzNDJlMzBaWTkzVk9WdFFHWGY0c3RuaWlnY1VOWWN4dVlVZ1RSQzViS3ZPSUgvWHFNPQ==;MTE1Mzg4NUAzMjMwMmUzNDJlMzBHTWxsL1czYm5uRkhEUndseUIzRFlvbGhZeWtNNGM5cnJhQmttYjlXckdZPQ==;MTE1Mzg4NkAzMjMwMmUzNDJlMzBUTFRFdllBQ1haUEZEeGgwMnZ1WUlQQnlteUtvMHprUWhKVlpYKzBiNFg0PQ==");

builder.Services.AddScoped<IClipboardService, ClipboardService>();

var host = builder.Build();

var storageService = host.Services.GetRequiredService<IClientPreferenceManager>();
if (storageService != null)
{
    CultureInfo culture;
    if (await storageService.GetPreference() is ClientPreference preference)
        culture = new CultureInfo(preference.LanguageCode);
    else
        culture = new CultureInfo(LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? "en-US");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

await host.RunAsync();