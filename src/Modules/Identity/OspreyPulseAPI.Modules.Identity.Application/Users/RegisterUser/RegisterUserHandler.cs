using MediatR;
using OspreyPulseAPI.Modules.Identity.Application.Abstractions;
using OspreyPulseAPI.Modules.Identity.Domain;

namespace OspreyPulseAPI.Modules.Identity.Application.Users.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityDbContext _context;

    public RegisterUserHandler(IIdentityDbContext context) => _context = context;

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var user = new User
        {
            Id = request.Id,
            Username = request.Username,
            Email = request.Email,
            OspreyPoints = 1000,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }
}
