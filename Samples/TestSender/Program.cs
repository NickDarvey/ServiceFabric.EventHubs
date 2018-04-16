using System;
using Microsoft.Azure.EventHubs;

namespace TestSender
{
    class Program
    {
        static void Main(string[] args)
        {
            EventHubClient
                .CreateFromConnectionString("")
                .SendAsync(new EventData(new byte [] { 0xDE, 0xAD }))
                .GetAwaiter()
                .GetResult();

            Console.ReadLine();
        }
    }
}
