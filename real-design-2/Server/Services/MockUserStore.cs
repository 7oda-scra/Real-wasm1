using RealDesign2.Shared.Models;

namespace RealDesign2.Server.Services;

public sealed class MockUserStore
{
    private readonly List<UserRecord> _users =
    [
        new(
            Username: "Mahmoud",
            Password: "1",
            User: new UserDto
            {
                Id = "usr-1",
                Username = "Mahmoud",
                FullName = "System Administrator",
                Role = "Admin",
                AllowedModuleIds =
                [
                    "pos",
                    "inventory",
                    "hr",
                    "sales",
                    "purchases",
                    "crm",
                    "exports-imports"
                ]
            }),
        new(
            Username: "sales",
            Password: "1",
            User: new UserDto
            {
                Id = "usr-2",
                Username = "sales",
                FullName = "Alexandra Smith",
                Role = "Sales Representative",
                AllowedModuleIds =
                [
                    "pos",
                    "crm",
                    "sales"
                ]
            }),
        new(
            Username: "hr",
            Password: "1",
            User: new UserDto
            {
                Id = "usr-3",
                Username = "hr",
                FullName = "Jordan Lee",
                Role = "HR Manager",
                AllowedModuleIds =
                [
                    "hr"
                ]
            })
    ];

    public Task<UserDto?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = _users.FirstOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
            user.Password == password);

        return Task.FromResult(record is null ? null : Clone(record.User));
    }

    public Task<UserDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = _users.FirstOrDefault(user => string.Equals(user.User.Id, id, StringComparison.Ordinal));
        return Task.FromResult(record is null ? null : Clone(record.User));
    }

    private static UserDto Clone(UserDto user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            AllowedModuleIds = [.. user.AllowedModuleIds]
        };
    }

    private sealed record UserRecord(string Username, string Password, UserDto User);
}
