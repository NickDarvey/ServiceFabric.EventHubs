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
        public Task<IActionResult> ReceiveEvents([FromBody]Cat cat)
        {
            if (!ModelState.IsValid || cat == null) return Task.FromResult<IActionResult>(BadRequest(ModelState));
            ServiceEventSource.Current.Message($"Received a cat named '{cat.Name}'");
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("poison")]
        public async Task<IActionResult> ReceiveErrors()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var msg = await reader.ReadToEndAsync();
                ServiceEventSource.Current.Error($"Received a poison message from a {Request.Headers["X-Poison-StatusCode"]}: {msg}");
            }

            return Ok();
        }
    }
}
