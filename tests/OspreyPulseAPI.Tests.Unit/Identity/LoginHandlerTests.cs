using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Application.Users.Login;
using OspreyPulseAPI.Modules.Identity.Domain;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;

namespace OspreyPulseAPI.Tests.Unit.Identity;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_LoginByEmail_ReturnsAccessTokenAndProfile()
    {
        var email = "test@example.com";
        var password = "SecurePass123!";
        var accessToken = "jwt-token-123";
        var username = "testuser";
        var ospreyPoints = 1000L;

        var supabase = Substitute.For<ISupabaseAuthService>();
        supabase.SignInAsync(email, password, Arg.Any<CancellationToken>()).Returns(accessToken);

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            OspreyPoints = ospreyPoints,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new LoginHandler(supabase, context);
        var command = new LoginCommand(email, password);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(username, result.Username);
        Assert.Equal(ospreyPoints, result.OspreyPoints);
    }

    [Fact]
    public async Task Handle_LoginByUsername_ResolvesEmail_ReturnsAccessTokenAndProfile()
    {
        var username = "kiwi_warrior";
        var email = "tester@example.com";
        var password = "SecurePass123!";
        var accessToken = "jwt-token-456";
        var ospreyPoints = 1500L;

        var supabase = Substitute.For<ISupabaseAuthService>();
        supabase.SignInAsync(email, password, Arg.Any<CancellationToken>()).Returns(accessToken);

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            OspreyPoints = ospreyPoints,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new LoginHandler(supabase, context);
        var command = new LoginCommand(username, password);

        var result = await handler.Handle(command, CancellationToken.None);

        await supabase.Received(1).SignInAsync(email, password, Arg.Any<CancellationToken>());
        Assert.Equal(accessToken, result.AccessToken);
        Assert.Equal(username, result.Username);
        Assert.Equal(ospreyPoints, result.OspreyPoints);
    }

    [Fact]
    public async Task Handle_LoginByUnknownUsername_Throws()
    {
        var supabase = Substitute.For<ISupabaseAuthService>();
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        var handler = new LoginHandler(supabase, context);
        var command = new LoginCommand("nonexistent_user", "password");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
        await supabase.DidNotReceive().SignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
