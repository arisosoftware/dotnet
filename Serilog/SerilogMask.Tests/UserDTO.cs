using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using SerilogMask.Core.Logging;
using System.Text.Json;

namespace SerilogMask.Tests;




public class UserDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

}
