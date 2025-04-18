using FakeMail.Database;
using FakeMail.Database.Models;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines; // Add this namespace for PipeReader.AsStream()

public class MailStore : MessageStore
{
    private readonly DbContextOptions<FakeMailDbContext> _options;

    public MailStore(DbContextOptions<FakeMailDbContext> options)
    {
        _options = options;
    }


    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        await stream.WriteAsync(buffer.ToArray(), cancellationToken); // Convert ReadOnlySequence<byte> to a stream
        stream.Position = 0; // Reset stream position to the beginning

        var message = MimeMessage.Load(stream);

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
            await db.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"Mail received for {email.Recipient}: {email.Subject}");
        }

        return SmtpResponse.Ok;
    }
     
}
