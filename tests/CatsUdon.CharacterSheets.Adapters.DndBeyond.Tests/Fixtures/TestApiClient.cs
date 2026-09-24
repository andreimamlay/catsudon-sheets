using CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;
using System.Text.Json;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond.Tests.Fixtures;

internal readonly struct TestClientConfiguration
{
    public required string CharacterJsonFile { get; init; }
    public required (int classId, int level, string fileName)[] AlwaysPreparedSpellsJsonFiles { get; init; }
}

internal class TestApiClient(TestClientConfiguration configuration) : IDndBeyondApiClient
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<AlwaysPreparedSpell[]> GetAlwaysPreparedSpellsAsync(int classId, int classLevel)
    {
        var file = configuration.AlwaysPreparedSpellsJsonFiles
            .Where(s => s.classId == classId && s.level == classLevel)
            .Select(s => s.fileName)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(file))
        {
            return [];
        }

        var sample = await File.ReadAllTextAsync(file);
        var data = JsonSerializer.Deserialize<ApiResponse<AlwaysPreparedSpell[]>>(sample, jsonSerializerOptions);
        if (data == null || !data.Success || data.Data == null)
        {
            throw new InvalidOperationException("Failed to retrieve character data");
        }

        return data.Data;
    }

    public async Task<CharacterData> GetCharacterAsync(string characterId)
    {
        var sample = await File.ReadAllTextAsync(configuration.CharacterJsonFile);
        var data = JsonSerializer.Deserialize<ApiResponse<CharacterData>>(sample, jsonSerializerOptions);
        if (data == null || !data.Success || data.Data == null)
        {
            throw new InvalidOperationException("Failed to retrieve character data");
        }

        return data.Data;
    }
}
