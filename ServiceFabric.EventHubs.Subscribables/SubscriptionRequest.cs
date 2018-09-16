using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NickDarvey.ServiceFabric.EventHubs
{
    [DataContract]
    public class SubscriptionRequest
    {
        [DataMember]
        public string SubscriptionName { get; private set; }

        public SubscriptionRequest(string subscriptionName)
        {
            SubscriptionName = subscriptionName;
        }
    }
}
