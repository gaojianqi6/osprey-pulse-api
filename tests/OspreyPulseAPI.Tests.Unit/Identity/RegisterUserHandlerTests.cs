using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;

namespace OspreyPulseAPI.Tests.Unit.Identity;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_SignsUpInSupabase_AddsUser_AndReturnsId()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var password = "SecurePass123!";

        var supabase = Substitute.For<ISupabaseAuthService>();
        supabase.SignUpAsync(email, password, Arg.Any<IReadOnlyDictionary<string, string>?>(), Arg.Any<CancellationToken>())
            .Returns(userId);

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        var handler = new RegisterUserHandler(supabase, context);
        var command = new RegisterUserCommand(username, email, password);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(userId, result);
        var user = await context.Users.SingleAsync();
        Assert.Equal(userId, user.Id);
        Assert.Equal(username, user.Username);
        Assert.Equal(email, user.Email);
        Assert.Equal(1000, user.OspreyPoints);
    }
}
