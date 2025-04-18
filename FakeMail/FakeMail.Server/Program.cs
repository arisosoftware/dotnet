using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using Microsoft.EntityFrameworkCore;
using FakeMail.Database;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        // Initialize the DbContext options
        var dbOptions = new DbContextOptionsBuilder<FakeMailDbContext>()
            .UseSqlite("Data Source=fakemail.db")
            .Options;

        // Ensure database is created
        using (var db = new FakeMailDbContext(dbOptions))
        {
            db.Database.EnsureCreated();
        }

        // Initialize MailStore
        var mailStore = new MailStore(dbOptions);

        // Create a service provider to resolve dependencies
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMessageStore>(mailStore)
            .BuildServiceProvider();

        // Create SMTP server options
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Port(2525)
            .Build();

        // Initialize the SMTP server
        var server = new SmtpServer.SmtpServer(options, serviceProvider);

        // Start the SMTP server
        Console.WriteLine("SMTP server listening on port 2525...");
        await server.StartAsync(System.Threading.CancellationToken.None);

        // Keep the server running
        Console.ReadLine();
    }
}
