using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using Microsoft.EntityFrameworkCore;
using FakeMail.Database;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)  // Main now asynchronous
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

        // Initialize MailStore (synchronously)
        var mailStore = new MailStore(dbOptions);

        // Create SMTP server options (synchronously)
        var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Port(2525)
            .Build();

        // Initialize the SMTP server
        var server = new SmtpServer.SmtpServer(options, mailStore);

        // Start the SMTP server (await the async method)
        Console.WriteLine("SMTP server listening on port 2525...");
        await server.StartAsync(System.Threading.CancellationToken.None);  // Start the server asynchronously

        // Keep the server running (infinite loop)
        Console.ReadLine();
    }
}
