using SmtpServer;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using FakeMail.Database;
using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class MailStore : MessageStore
{
    private readonly DbContextOptions<FakeMailDbContext> _options;

    public MailStore(DbContextOptions<FakeMailDbContext> options)
    {
        _options = options;
    }

    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        var message = MimeMessage.Load(buffer.AsStream());

        using (var db = new FakeMailDbContext(_options))
        {
            var email = new EmailMessage
            {
                Sender = transaction.From.AsAddress().ToString(),
                Recipient = string.Join(", ", transaction.To.Select(t => t.AsAddress().ToString())),
                Subject = message.Subject ?? "(no subject)",
                Body = message.TextBody ?? message.HtmlBody ?? "",
                ReceivedAt = DateTime.UtcNow
            };

            db.Emails.Add(email);
            await db.SaveChangesAsync(cancellationToken);  // Async save to the database

            Console.WriteLine($"Mail received for {email.Recipient}: {email.Subject}");
        }

        return SmtpResponse.Ok;
    }
}
