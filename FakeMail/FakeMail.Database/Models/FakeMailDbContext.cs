using Microsoft.EntityFrameworkCore;
using FakeMail.Database.Models;
using System.Buffers;
namespace FakeMail.Database;

public class FakeMailDbContext : DbContext
{
    public FakeMailDbContext(DbContextOptions<FakeMailDbContext> options) : base(options) { }

    public DbSet<EmailMessage> Emails => Set<EmailMessage>();
}