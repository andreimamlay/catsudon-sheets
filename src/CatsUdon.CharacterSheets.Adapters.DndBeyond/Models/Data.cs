namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class Data
{
    public required string Name { get; set; }
    public int BaseHitPoints { get; set; }
    public int BonusHitPoints { get; set; }
    public int RemovedHitPoints { get; set; }
    public int TemporaryHitPoints { get; set; }
    public required Stat[] Stats { get; set; }
    public required Stat[] BonusStats { get; set; }
    public required Stat[] OverrideStats { get; set; }
    public required ModifiersTable Modifiers { get; set; }
    public required Class[] Classes { get; set; }
}
