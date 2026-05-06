using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond;

internal partial class DndBeyondAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [GeneratedRegex(@"^https:\/\/www\.dndbeyond\.com\/characters\/(?<characterId>\d+)$")]
    private static partial Regex UrlMatchRegex { get; }

    private const string CharacterApiUrlTemplate = "https://character-service.dndbeyond.com/character/v5/character/{0}?includeCustomItems=true";

    private static readonly Lazy<GameSystemInfo[]> supportedSystems = new([
        new GameSystemInfo()
        {
            ProviderName = "D&D Beyond",
            ProviderHomePageUrl = new Uri("https://www.dndbeyond.com/"),
            GameSystemName = "D&D 5e",
            MainPageUrl = new Uri("https://www.dndbeyond.com/"),
            CharacterCreationUrl = new Uri("https://www.dndbeyond.com/characters/builder"),
        },
        new GameSystemInfo()
        {
            ProviderName = "D&D Beyond",
            ProviderHomePageUrl = new Uri("https://www.dndbeyond.com/"),
            GameSystemName = "D&D 5.5e",
            MainPageUrl = new Uri("https://www.dndbeyond.com/"),
            CharacterCreationUrl = new Uri("https://www.dndbeyond.com/characters/builder"),
        }
    ]);

    public GameSystemInfo[] SupportedGameSystems => supportedSystems.Value;

    public bool CanConvert(string url) => UrlMatchRegex.IsMatch(url);

    public async Task<CharacterSheet> Convert(string url)
    {
        if (!CanConvert(url))
        {
            throw new ArgumentException("URL is not supported", nameof(url));
        }

        var match = UrlMatchRegex.Match(url);
        var characterId = match.Groups["characterId"].Value;

        //var getCharacterJsonResponse = await httpClient.GetAsync(string.Format(CharacterApiUrlTemplate, characterId));
        //getCharacterJsonResponse.EnsureSuccessStatusCode();

        //var response = await getCharacterJsonResponse.Content.ReadAsStringAsync();

        var sample = await File.ReadAllTextAsync("../CatsUdon.CharacterSheets.Adapters.DndBeyond/sample.json");
        var data = JsonSerializer.Deserialize<ApiResponse>(sample, jsonSerializerOptions);
        if (data == null || !data.Success || data.Data == null)
        {
            throw new InvalidOperationException("Failed to retrieve character data");
        }

        var character = new Character();
        ReadBaseDetails(data.Data, character);
        ReadAC(data.Data, character);
        ReadWeaponAttacks(data.Data, character);

        return null;
    }

    private static void ReadWeaponAttacks(Data data, Character character)
    {
        var equippedWeapons = data.Inventory
            .Where(e => e.Equipped)
            .Where(e => e.Definition.AttackType.HasValue)
            .Where(e => !e.Definition.CanAttune || e.IsAttuned)
            .ToArray();

        foreach (var weapon in equippedWeapons)
        {
            if (weapon.Definition.Damage == null) continue;
            if (!weapon.Definition.CategoryId.HasValue) continue;

            var damageDie = new Die()
            {
                Count = weapon.Definition.Damage.DiceCount,
                Sides = weapon.Definition.Damage.DiceValue
            };

            var isFinesseWeapon = weapon.Definition.Properties.Any(p => p.Name == "Finesse");
            var attackBonus = isFinesseWeapon switch
            {
                true => Math.Max(character.StrengthModifier, character.DexterityModifier),
                false => character.StrengthModifier
            };

            var weaponCategory = weapon.Definition.CategoryId.Value switch
            {
                CategoryId.Simple => "simple-weapons",
                CategoryId.Martial => "martial-weapons",
                _ => ""
            };
            var weaponCategoryProficiency = GetProficiencyBonus(data.Modifiers, weaponCategory, character.Level);
            var weaponProficiency = GetProficiencyBonus(data.Modifiers, weapon.Definition.Name.ToLower(), character.Level);

            attackBonus += Math.Max(weaponCategoryProficiency, weaponProficiency);

            character.Attacks.Add(new Attack()
            {
                Name = weapon.Definition.Name,
                AttackBonus = new Modifier(attackBonus),
                Damage = damageDie
            });

            var versatileProperty = weapon.Definition.Properties.FirstOrDefault(p => p.Name == "Versatile");
            if (versatileProperty != null)
            {
                if (Die.TryParse(versatileProperty.Notes, out var versatileDamageDie))
                {
                    character.Attacks.Add(new Attack()
                    {
                        Name = weapon.Definition.Name,
                        AttackBonus = new Modifier(attackBonus),
                        Damage = versatileDamageDie.Value
                    });
                }
            }
        }
    }

    private static void ReadAC(Data data, Character character)
    {
        var equippedArmor = data.Inventory
            .Where(e => e.Equipped)
            .Where(e => e.Definition.ArmorTypeId.HasValue && e.Definition.ArmorTypeId != ArmorType.Shield)
            .Where(e => !e.Definition.CanAttune || e.IsAttuned)
            .FirstOrDefault();

        var equippedShield = data.Inventory
            .Where(e => e.Equipped)
            .Where(e => e.Definition.ArmorTypeId == ArmorType.Shield)
            .Where(e => !e.Definition.CanAttune || e.IsAttuned)
            .FirstOrDefault();

        var hasUnarmoredDefense = data.Modifiers.Class.Any(m => m.Type == Types.Set && m.SubType == "unarmored-armor-class");
        if (hasUnarmoredDefense && equippedArmor == null)
        {
            character.ArmorClass = 10 + character.DexterityModifier + character.ConstitutionModifier;
        }
        else
        {
            var armorClass = equippedArmor?.Definition.ArmorClass ?? 10;
            if (equippedArmor?.Definition.ArmorTypeId != ArmorType.Heavy) armorClass += Math.Min(character.DexterityModifier, 2);
            if (equippedShield != null) armorClass += equippedShield.Definition.ArmorClass ?? 0;

            character.ArmorClass = armorClass;
        }
    }

    private static void ReadBaseDetails(Data data, Character character)
    {
        character.Name = data.Name;
        character.Level = data.Classes.Sum(c => c.Level);
        character.HitDice = [.. data.Classes.Select(c => new Die() { Count = c.Level, Sides = c.Definition.HitDice })];

        character.StrengthScore = SumAbilityScores(StatIds.Strength, data);
        character.DexterityScore = SumAbilityScores(StatIds.Dexterity, data);
        character.ConstitutionScore = SumAbilityScores(StatIds.Constitution, data);
        character.IntelligenceScore = SumAbilityScores(StatIds.Intelligence, data);
        character.WisdomScore = SumAbilityScores(StatIds.Wisdom, data);
        character.CharismaScore = SumAbilityScores(StatIds.Charisma, data);

        character.StrengthModifier = ScoreToModifier(character.StrengthScore);
        character.DexterityModifier = ScoreToModifier(character.DexterityScore);
        character.ConstitutionModifier = ScoreToModifier(character.ConstitutionScore);
        character.IntelligenceModifier = ScoreToModifier(character.IntelligenceScore);
        character.WisdomModifier = ScoreToModifier(character.WisdomScore);
        character.CharismaModifier = ScoreToModifier(character.CharismaScore);

        character.StrengthSavingThrowModifier = character.StrengthModifier + GetProficiencyBonus(data.Modifiers, "strength-saving-throws", character.Level);
        character.DexteritySavingThrowModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "dexterity-saving-throws", character.Level);
        character.ConstitutionSavingThrowModifier = character.ConstitutionModifier + GetProficiencyBonus(data.Modifiers, "constitution-saving-throws", character.Level);
        character.IntelligenceSavingThrowModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "intelligence-saving-throws", character.Level);
        character.WisdomSavingThrowModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "wisdom-saving-throws", character.Level);
        character.CharismaSavingThrowModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "charisma-saving-throws", character.Level);

        character.AcrobaticsModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "acrobatics", character.Level);
        character.AnimalHandlingModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "animal-handling", character.Level);
        character.ArcanaModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "arcana", character.Level);
        character.AthleticsModifier = character.StrengthModifier + GetProficiencyBonus(data.Modifiers, "athletics", character.Level);
        character.DeceptionModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "deception", character.Level);
        character.HistoryModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "history", character.Level);
        character.InsightModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "insight", character.Level);
        character.IntimidationModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "intimidation", character.Level);
        character.InvestigationModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "investigation", character.Level);
        character.MedicineModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "medicine", character.Level);
        character.NatureModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "nature", character.Level);
        character.PerceptionModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "perception", character.Level);
        character.PerformanceModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "performance", character.Level);
        character.PersuasionModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "persuasion", character.Level);
        character.ReligionModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "religion", character.Level);
        character.SleightOfHandModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "sleight-of-hand", character.Level);
        character.StealthModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "stealth", character.Level);
        character.SurvivalModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "survival", character.Level);

        character.PassivePerception = 10 + character.PerceptionModifier;
        character.MaxHp = CalculateMaxHp(data, character);
        character.CurrentHp = character.MaxHp - data.RemovedHitPoints;
        character.TemporaryHp = data.TemporaryHitPoints;
    }

    private static int CalculateMaxHp(Data data, Character character)
    {
        var classes = data.Classes;
        var totalHp = 0;

        foreach (var cls in classes)
        {
            int level = cls.Level;
            int hitDie = cls.Definition.HitDice;
            int fixedPerLevel = (hitDie / 2) + 1;

            if (cls.IsStartingClass)
            {
                // lvl1
                totalHp += hitDie + character.ConstitutionModifier;

                // lvl2+
                totalHp += (level - 1) * (fixedPerLevel + character.ConstitutionModifier);
            }
            else
            {
                totalHp += level * (fixedPerLevel + character.ConstitutionModifier);
            }
        }

        return totalHp;
    }

    private static int GetProficiencyBonus(ModifiersTable modifiers, string subType, int characterLevel)
    {
        var isProficient = modifiers.Race.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Class.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Background.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Item.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Feat.Any(m => m.Type == Types.Proficiency && m.SubType == subType);

        if (!isProficient) return 0;

        return 1 + (int)Math.Ceiling(characterLevel / 4f);
    }

    private static int SumAbilityScores(StatIds statId, Data data)
    {
        var overrideStat = data.OverrideStats.FirstOrDefault(s => s.Id == statId);
        if (overrideStat != null && overrideStat.Value.HasValue) return overrideStat.Value.Value;

        var score = 0;
        score += data.Stats.First(s => s.Id == statId).Value ?? 0;
        score += data.BonusStats.First(s => s.Id == statId).Value ?? 0;
        score += SumModifiers(data.Modifiers.Race, Types.Bonus, GetSubTypeForStat(statId));
        score += SumModifiers(data.Modifiers.Class, Types.Bonus, GetSubTypeForStat(statId));
        score += SumModifiers(data.Modifiers.Background, Types.Bonus, GetSubTypeForStat(statId));
        score += SumModifiers(data.Modifiers.Item, Types.Bonus, GetSubTypeForStat(statId));
        score += SumModifiers(data.Modifiers.Feat, Types.Bonus, GetSubTypeForStat(statId));

        return score;

        static string GetSubTypeForStat(StatIds statId) => statId switch
        {
            StatIds.Strength => SubTypes.StrengthScore,
            StatIds.Dexterity => SubTypes.DexterityScore,
            StatIds.Constitution => SubTypes.ConstitutionScore,
            StatIds.Intelligence => SubTypes.IntelligenceScore,
            StatIds.Wisdom => SubTypes.WisdomScore,
            StatIds.Charisma => SubTypes.CharismaScore,
            _ => throw new InvalidOperationException($"Unknown stat id: {statId}")
        };
    }

    private static int SumModifiers(CharacterModifier[] modifiers, string type, string subType) => modifiers.Where(m => m.Type == type && m.SubType == subType).Sum(m => m.FixedValue ?? 0);
    private static int ScoreToModifier(int score) => (int)Math.Floor((score - 10) / 2f);
}
