using ZANECO.WebApi.Shared.Notifications;

namespace ZANECO.WASM.Client.Infrastructure.Notifications;
public record ConnectionStateChanged(ConnectionState State, string? Message) : INotificationMessage;