namespace RealDesign2.Shared.Models;

public sealed class ModuleDefinitionDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string IconPath { get; set; }

    public required string Route { get; set; }

    public required string Description { get; set; }

    public required string HexColor { get; set; }
}
