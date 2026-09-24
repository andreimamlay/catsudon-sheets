namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Tests;

[Trait("Adapter", "Dnd Beyond")]
public class SingleclassTests
{
    [Fact(DisplayName = "Singleclass 164247771")]
    public async Task IsWorking()
    {
        var client = new TestApiClient(new TestClientConfiguration()
        {
            CharacterJsonFile = "TestData/single-164247771.json",
            AlwaysPreparedSpellsJsonFiles = []
        });
        var adapter = new DndBeyondAdapter(client);

        var result = await adapter.Convert("https://www.dndbeyond.com/characters/164247771");
    }
}
