using Blazor.ClientStorage;
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

builder.Services.AddSyncfusionBlazor();
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBaFt+QHFqVkNrXVNbdV5dVGpAd0N3RGlcdlR1fUUmHVdTRHRcQl5hTH5Qc01gXHtccnc=;Mgo+DSMBPh8sVXJ1S0d+X1RPd11dXmJWd1p/THNYflR1fV9DaUwxOX1dQl9gSX1Rc0VgXHdddXBWQ2M=;ORg4AjUWIQA/Gnt2VFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5QdEBjWnpWc3FRQmlf;MTQ2MzY4N0AzMjMxMmUzMTJlMzMzNWVsNFlCQm15N3FiaCtSYVB4RmxpQjZrM0ptK3hkSWErTVNuekk5ZERYeTQ9;MTQ2MzY4OEAzMjMxMmUzMTJlMzMzNWFoWXF0cGg4R2V6K2ZxRWF5WGdoOFdYNjNkQkNDZGJwZkErdmZzeDlOQjA9;NRAiBiAaIQQuGjN/V0d+XU9Hc1RDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TdUdmWXxbeHdQQ2JcUg==;MTQ2MzY5MEAzMjMxMmUzMTJlMzMzNWdTT2c0Wkx4QVZiSFNNRTcrWDI2UGJvRFU4bERmaVlid0NPM2hVNjlZa2s9;MTQ2MzY5MUAzMjMxMmUzMTJlMzMzNUo2TlFMdDl6TmtQSWtNQW41T3IwZzhta0R5dm5TWDcxMDdmazFSMnorSU09;Mgo+DSMBMAY9C3t2VFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hSn5QdEBjWnpWc3FQTmhf;MTQ2MzY5M0AzMjMxMmUzMTJlMzMzNWljdTJheEFHQm1YNm0vYjJ0OW84aHFXSklGYzNnOWFqcXVzMWtTelcraWM9;MTQ2MzY5NEAzMjMxMmUzMTJlMzMzNWJWNXZMcmUwWkRaN1c2c3E0cFArS01aVXBncVZDdUZXL2FIenRjT0wvbmc9;MTQ2MzY5NUAzMjMxMmUzMTJlMzMzNWdTT2c0Wkx4QVZiSFNNRTcrWDI2UGJvRFU4bERmaVlid0NPM2hVNjlZa2s9");

builder.Services.AddScoped<IClipboardService, ClipboardService>();

builder.Services.AddBlazorClientStorage();

var host = builder.Build();

var storageService = host.Services.GetRequiredService<IClientPreferenceManager>();
if (storageService is not null)
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