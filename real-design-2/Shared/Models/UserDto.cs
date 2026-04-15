namespace RealDesign2.Shared.Models;

public sealed class UserDto
{
    public required string Id { get; set; }

    public required string Username { get; set; }

    public required string FullName { get; set; }

    public required string Role { get; set; }

    public required List<string> AllowedModuleIds { get; set; } = new();
}
