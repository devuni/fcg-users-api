using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Users.Application.Interfaces;

namespace Users.Infrastructure.Messaging;

public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T message, CancellationToken ct) where T : class
        => _publishEndpoint.Publish(message, ct);
}
