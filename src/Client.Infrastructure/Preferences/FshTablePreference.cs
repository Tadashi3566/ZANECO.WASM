using ZANECO.WebApi.Shared.Notifications;

namespace ZANECO.WASM.Client.Infrastructure.Preferences;
public class FshTablePreference : INotificationMessage
{
    public bool IsFixedHeaderFooter { get; set; }
    public bool IsAllowUnsorted { get; set; }
    public bool IsDense { get; set; }
    public bool IsStriped { get; set; }
    public bool HasBorder { get; set; }
    public bool IsHoverable { get; set; }
    public bool IsMultiSelection { get; set; }
    public bool IsVirtualize { get; set; }
}