namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;

internal class ModifiersTable
{
    public Modifier[] Race { get; set; } = [];
    public Modifier[] Class { get; set; } = [];
    public Modifier[] Background { get; set; } = [];
    public Modifier[] Item { get; set; } = [];
    public Modifier[] Feat { get; set; } = [];
}
