namespace RealDesign2.Client.Workspace;

public sealed class WorkspaceTab
{
    public required string Id { get; init; }

    public required string Title { get; set; }

    public string? Subtitle { get; set; }

    public required string Icon { get; init; }

    public string? Route { get; init; }

    public required Type ComponentType { get; init; }

    public Dictionary<string, object?> Parameters { get; init; } = [];

    public bool IsDirty { get; set; }

    public bool IsPinned { get; init; }

    public DateTimeOffset OpenedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastActivatedAt { get; set; } = DateTimeOffset.UtcNow;
}
