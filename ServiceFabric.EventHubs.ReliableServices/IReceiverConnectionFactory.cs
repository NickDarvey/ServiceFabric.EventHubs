using System.Threading;
using System.Threading.Tasks;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public interface IReceiverConnectionFactory
    {
        Task<IReceiverConnection> CreateReceiver(
            long partitionKey, string consumerGroupName, CancellationToken cancel = default);
    }
}
