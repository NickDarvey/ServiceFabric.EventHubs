using System.ComponentModel.DataAnnotations;

namespace NickDarvey.SampleApplication.ReliableService.AspNetCore.Models
{
    public class Cat
    {
        [Required] public string Name { get; set;  }
    }
}
