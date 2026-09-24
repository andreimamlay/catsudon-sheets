using CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond;

internal interface IDndBeyondApiClient
{
    Task<CharacterData> GetCharacterAsync(string characterId);
    Task<AlwaysPreparedSpell[]> GetAlwaysPreparedSpellsAsync(int classId, int classLevel);
}

internal class DndBeyondApiClient(HttpClient httpClient) : IDndBeyondApiClient
{
    private const string CharacterApiUrlTemplate = "https://character-service.dndbeyond.com/character/v5/character/{0}?includeCustomItems=true";
    private const string AlwaysPreparedSpellsUriTemplate = "https://character-service.dndbeyond.com/character/v5/game-data/always-prepared-spells?sharingSetting=2&classId={0}&classLevel={1}";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);


    public async Task<AlwaysPreparedSpell[]> GetAlwaysPreparedSpellsAsync(int classId, int classLevel)
    {
        var getCharacterJsonResponse = await httpClient.GetAsync(string.Format(AlwaysPreparedSpellsUriTemplate, classId, classLevel));
        getCharacterJsonResponse.EnsureSuccessStatusCode();

        var apiResponse = await getCharacterJsonResponse.Content.ReadFromJsonAsync<ApiResponse<AlwaysPreparedSpell[]>>(SerializerOptions);
        if (apiResponse == null || apiResponse.Data == null)
        {
            throw new InvalidOperationException();
        }

        return apiResponse.Data;
    }

    public async Task<CharacterData> GetCharacterAsync(string characterId)
    {
        var getCharacterJsonResponse = await httpClient.GetAsync(string.Format(CharacterApiUrlTemplate, characterId));
        getCharacterJsonResponse.EnsureSuccessStatusCode();

        var apiResponse = await getCharacterJsonResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterData>>(SerializerOptions);
        if (apiResponse == null || apiResponse.Data == null)
        {
            throw new InvalidOperationException();
        }

        return apiResponse.Data;
    }

    private class ApiResponse<T>
    {
        public T? Data { get; set; }
    }
}
