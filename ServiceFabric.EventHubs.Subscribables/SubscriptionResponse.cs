using System.Runtime.Serialization;

namespace NickDarvey.ServiceFabric.EventHubs
{
    [DataContract]
    public class SubscriptionResponse
    {
        [DataMember]
        public string ConnectionString { get; private set; }
    }
}
