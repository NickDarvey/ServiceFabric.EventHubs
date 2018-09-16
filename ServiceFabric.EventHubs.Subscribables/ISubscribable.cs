using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public interface ISubscribable : IService
    {
        Task<SubscriptionResponse> Subscribe(SubscriptionRequest request);
    }
}
