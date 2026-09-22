namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Tests;

[Trait("Adapter", "Dnd Beyond")]
public class MulticlassTests
{
    [Fact(DisplayName = "Is Working")]
    public async Task IsWorking()
    {
        var client = new TestApiClient(new TestClientConfiguration()
        {
            CharacterJsonFile = "TestData/multiclass-character.json",
            AlwaysPreparedSpellsJsonFiles =
            [
                (19, 5, "TestData/multiclass-always-prepared-spells-19-5.json"),
                (254188, 3, "TestData/multiclass-always-prepared-spells-254188-3.json")
            ]
        });
        var adapter = new DndBeyondAdapter(client);

        var result = await adapter.Convert("https://www.dndbeyond.com/characters/165310013");
    }
}
