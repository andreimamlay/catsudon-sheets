namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ClassFeature
{
    public required ClassFeatureDefinition Definition { get; set; }
}

internal class ClassFeatureDefinition
{
    public required int Id { get; set; }
    public required int RequiredLevel { get; set; }
}
