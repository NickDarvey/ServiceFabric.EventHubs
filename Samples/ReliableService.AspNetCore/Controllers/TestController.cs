using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NickDarvey.SampleApplication.ReliableService.AspNetCore.Models;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore.Controllers
{
    [Produces("application/json")]
    [Route("test")]
    public class TestController : Controller
    {
        [HttpPost("events")]
        public Task ReceiveEvents(Cat cat)
        {
            ServiceEventSource.Current.Message($"Received a cat with the name '{cat.Name}'");
            return Task.CompletedTask;
        }

        [HttpPost("errors")]
        public Task ReceiveErrors(string error)
        {
            ServiceEventSource.Current.Error("Received an error: " + error);
            return Task.CompletedTask;
        }
    }
}
