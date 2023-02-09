using ZANECO.WebApi.Shared.Notifications;

namespace ZANECO.WASM.Client.Infrastructure.Preferences;

public class FshTablePreference : INotificationMessage
{
    public bool IsFixedHeaderFooter { get; set; } = true;
    public bool IsAllowUnsorted { get; set; } = true;
    public bool IsDense { get; set; } = true;
    public bool IsStriped { get; set; } = true;
    public bool HasBorder { get; set; } = true;
    public bool IsHoverable { get; set; } = true;
    public bool IsMultiSelection { get; set; }
    public bool IsVirtualize { get; set; } = true;
}

public class BackgroundPreference : INotificationMessage
{
    public bool IsBackgroundJob { get; set; }
    public bool IsScheduled { get; set; }
    public int InMinutes { get; set; }
}