using System.Threading;
using System.Threading.Tasks;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Messaging;

/// <summary>
/// Used when Messaging:EnableRabbitMQ=false (ex.: Cloud TC3 path using Service Bus + Functions).
/// </summary>
public sealed class NoopEventBus : IEventBus
{
    public Task PublishAsync<T>(T message, CancellationToken ct) where T : class
        => Task.CompletedTask;
}
