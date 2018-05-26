using System;
using System.IO;
using System.Text;
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
        public async Task ReceiveErrors()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var ex = await reader.ReadToEndAsync();
                ServiceEventSource.Current.Error("Received an error: " + ex);

            }
        }
    }
}
