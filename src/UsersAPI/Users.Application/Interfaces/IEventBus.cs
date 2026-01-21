using System.Threading;
using System.Threading.Tasks;

namespace Users.Application.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken ct) where T : class;
}
