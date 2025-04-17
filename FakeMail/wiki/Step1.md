Perfect! Let’s build your **minimal fake mail system** step by step using .NET 8.

We’ll name the solution `FakeMail`. It will:

✅ Accept SMTP mail using `SmtpServer`  
✅ Store messages in `SQLite`  
✅ Provide a **Blazor Server** Web UI to view mail per recipient  
✅ Allow logging in by entering an email address (no password)  

---

## 🚀 Step 1: Create Solution and Projects

```bash
mkdir FakeMail
cd FakeMail

dotnet new sln -n FakeMail

dotnet new console -n FakeMail.Server       # SMTP receiver
dotnet new blazorserver -n FakeMail.Web     # Web UI
dotnet new classlib -n FakeMail.Database    # Shared EF Models

dotnet sln add FakeMail.Server/FakeMail.Server.csproj
dotnet sln add FakeMail.Web/FakeMail.Web.csproj
dotnet sln add FakeMail.Database/FakeMail.Database.csproj

dotnet add FakeMail.Server reference FakeMail.Database
dotnet add FakeMail.Web reference FakeMail.Database
```

---

## 🧩 Step 2: Install NuGet Packages

### In `FakeMail.Server`:
```bash
cd FakeMail.Server

dotnet add package SmtpServer
dotnet add package MimeKit
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### In `FakeMail.Web`:
```bash
cd ../FakeMail.Web

dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

---

## 🧱 Step 3: Define Email DB Model

### 📄 `FakeMail.Database/Models/EmailMessage.cs`

```csharp
namespace FakeMail.Database.Models;

public class EmailMessage
{
    public int Id { get; set; }
    public string Recipient { get; set; } = "";
    public string Sender { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
```

### 📄 `FakeMail.Database/FakeMailDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using FakeMail.Database.Models;

namespace FakeMail.Database;

public class FakeMailDbContext : DbContext
{
    public FakeMailDbContext(DbContextOptions<FakeMailDbContext> options) : base(options) { }

    public DbSet<EmailMessage> Emails => Set<EmailMessage>();
}
```

---

## 📨 Step 4: Implement SMTP Mail Receiver

### 📄 `FakeMail.Server/Program.cs`

```csharp
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Storage;
using SmtpServer.Protocol;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using FakeMail.Database;
using FakeMail.Database.Models;

var dbOptions = new DbContextOptionsBuilder<FakeMailDbContext>()
    .UseSqlite("Data Source=fakemail.db")
    .Options;

using (var db = new FakeMailDbContext(dbOptions))
{
    db.Database.EnsureCreated();
}

var services = new ServiceProvider();
services.Add(new MailStore(dbOptions));

var options = new SmtpServerOptionsBuilder()
    .ServerName("localhost")
    .Port(2525)
    .Build();

var server = new SmtpServer.SmtpServer(options, services);
Console.WriteLine("SMTP server listening on port 2525...");
await server.StartAsync(CancellationToken.None);

class MailStore : MessageStore
{
    private readonly DbContextOptions<FakeMailDbContext> _options;

    public MailStore(DbContextOptions<FakeMailDbContext> options) => _options = options;

    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        var message = MimeMessage.Load(buffer.AsStream());

        using var db = new FakeMailDbContext(_options);
        var email = new EmailMessage
        {
            Sender = transaction.From.AsAddress().ToString(),
            Recipient = string.Join(", ", transaction.To.Select(t => t.AsAddress().ToString())),
            Subject = message.Subject ?? "(no subject)",
            Body = message.TextBody ?? message.HtmlBody ?? "",
            ReceivedAt = DateTime.UtcNow
        };

        db.Emails.Add(email);
        await db.SaveChangesAsync();

        Console.WriteLine($"Mail received for {email.Recipient}: {email.Subject}");
        return SmtpResponse.Ok;
    }
}
```

---

## 🖥️ Step 5: Web UI to View Emails

### 🛠 Modify `Program.cs` in `FakeMail.Web`

```csharp
using FakeMail.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<FakeMailDbContext>(options =>
    options.UseSqlite("Data Source=../FakeMail.Server/fakemail.db"));

var app = builder.Build();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

---

### 📄 Add Inbox Page in `Pages/Inbox.razor`

```razor
@page "/inbox"
@using FakeMail.Database
@using FakeMail.Database.Models
@inject FakeMailDbContext Db

<h3>Inbox</h3>

<input type="text" @bind="filter" placeholder="Enter your email..." class="form-control mb-2" />
<button @onclick="LoadEmails" class="btn btn-primary mb-3">View</button>

@if (emails == null)
{
    <p>No emails yet.</p>
}
else
{
    <ul>
        @foreach (var email in emails)
        {
            <li>
                <b>@email.Subject</b> from @email.Sender on @email.ReceivedAt
                <pre>@email.Body</pre>
            </li>
        }
    </ul>
}

@code {
    string filter = "";
    List<EmailMessage>? emails;

    async Task LoadEmails()
    {
        emails = await Db.Emails
            .Where(e => e.Recipient.Contains(filter))
            .OrderByDescending(e => e.ReceivedAt)
            .ToListAsync();
    }
}
```

---

## ✅ Final Step: Run Everything

In two terminals:

```bash
# Terminal 1 - start SMTP server
cd FakeMail.Server
dotnet run
```

```bash
# Terminal 2 - start Web UI
cd FakeMail.Web
dotnet run
```

Then:
1. Send SMTP mail to `localhost:2525` from your app
2. Open the browser: http://localhost:5000/inbox
3. Enter email address (e.g., `user@example.com`) and view messages

---

## 🧩 Next Steps (Optional)

- Add login by email (simple identity or token)
- Support HTML body display (MimeKit has both HTML/Text)
- Add search, tags, delete, attachments
- Deploy on internal server for shared QA team use

---

Would you like me to zip the full version of this and share the code, or continue refining specific parts (login, view formatting, etc.)?



Thanks for sharing the full error and code. This confirms the Entity Framework Core package is still missing from your `FakeMail.Database` project.

Let’s fix this step-by-step:

---

## ✅ Step-by-Step Fix

### **Step 1: Add EF Core Packages to `FakeMail.Database`**

Run these commands **inside** the `FakeMail.Database` folder:

```bash
cd ~/dotnet/FakeMail/FakeMail.Database

dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

These two packages will give you:
- `DbContext`, `DbSet`, and `DbContextOptions<>`
- SQLite support
- EF Design-time tools (optional but useful)

---

### **Step 2: Rebuild Solution**

Now go back to the root folder and build everything:

```bash
cd ~/dotnet/FakeMail
dotnet build
```

---

### ✅ It should now compile without errors.

Let me know if you still see errors after this — we can go deeper if needed.