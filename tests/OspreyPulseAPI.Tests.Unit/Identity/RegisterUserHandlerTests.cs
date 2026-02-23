using Microsoft.EntityFrameworkCore;
using OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;
using OspreyPulseAPI.Modules.Identity.Infrastructure.Persistence;

namespace OspreyPulseAPI.Tests.Unit.Identity;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Handle_AddsUser_AndReturnsId()
    {
        var id = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new IdentityDbContext(options);
        var handler = new RegisterUserHandler(context);
        var command = new RegisterUserCommand(id, username, email);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(id, result);
        var user = await context.Users.SingleAsync();
        Assert.Equal(id, user.Id);
        Assert.Equal(username, user.Username);
        Assert.Equal(email, user.Email);
        Assert.Equal(1000, user.OspreyPoints);
    }
}
