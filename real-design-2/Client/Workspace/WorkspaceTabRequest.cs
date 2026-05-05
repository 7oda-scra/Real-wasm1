namespace RealDesign2.Client.Workspace;

public sealed class WorkspaceTabRequest
{
    public required string Id { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public required string Icon { get; init; }

    public string? Route { get; init; }

    public required Type ComponentType { get; init; }

    public Dictionary<string, object?> Parameters { get; init; } = [];

    public bool IsPinned { get; init; }
}
