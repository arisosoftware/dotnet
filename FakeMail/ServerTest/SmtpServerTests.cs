using System.Net.Mail;
using System.Threading.Tasks;
using Xunit;

public class SmtpServerTests
{
    [Fact]
    public async Task SmtpServer_ShouldAcceptEmail()
    {
        // Arrange
        var smtpClient = new SmtpClient("localhost", 2525)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var mailMessage = new MailMessage("sender@example.com", "recipient@example.com")
        {
            Subject = "Test Email",
            Body = "This is a test email."
        };

        // Act
        await smtpClient.SendMailAsync(mailMessage);

        // Assert
        // Here, you would query the database or mock the MailStore to verify the email was saved.
        // Example: Assert.True(emailWasSaved);
    }
}
