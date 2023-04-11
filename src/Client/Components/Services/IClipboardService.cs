namespace ZANECO.WASM.Client.Components.Services;

public interface IClipboardService
{
    Task CopyToClipboard(string text);
}