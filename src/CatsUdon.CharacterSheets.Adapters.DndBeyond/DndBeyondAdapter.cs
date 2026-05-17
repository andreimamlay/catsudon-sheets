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
        ReadAc(data.Data, character);
        ReadWeaponAttacks(data.Data, character);
        ReadSpellSlots(data.Data, character);
        ReadSpellEffects(data.Data, character);

        return null;
    }

    private void ReadSpellEffects(Data data, Character character)
    {
        var maxSlotLevel = GetMaxSpellSlotLevel(character);
        foreach (var classSpells in data.ClassSpells)
        {
            var characterClass = data.Classes.FirstOrDefault(c => c.Id == classSpells.CharacterClassId);
            if (characterClass == null) continue;

            var spellcastingModifier = characterClass.Definition.SpellCastingAbilityId switch
            {
                StatIds.Strength => character.StrengthModifier,
                StatIds.Dexterity => character.DexterityModifier,
                StatIds.Constitution => character.ConstitutionModifier,
                StatIds.Intelligence => character.IntelligenceModifier,
                StatIds.Wisdom => character.WisdomModifier,
                StatIds.Charisma => character.CharismaModifier,
                _ => 0
            };

            foreach (var spell in classSpells.Spells)
            {
                var damageModifier = spell.Definition.Modifiers.FirstOrDefault(m => m.Type == "damage");
                if (damageModifier == null) continue;

                if (damageModifier.AtHigherLevels.HigherLevelDefinitions.Length == 1)
                {
                    // Upcastable spell with scaling
                    for (int i = spell.Definition.Level; i < maxSlotLevel; i++)
                    {
                        if (spell.Definition.RequiresSavingThrow)
                        {
                            var spellEffect = new SpellEffect()
                            {
                                Name = spell.Definition.Name,
                                SpellSaveAbilityId = spell.Definition.SaveDcAbilityId,
                                SpellSaveDc = GetSpellSaveDc(spellcastingModifier, character),
                                Damage = GetUpcastDamageDie(damageModifier, spell.Definition.Level, i),
                                Level = i
                            };

                            character.SpellEffects.Add(spellEffect);
                        }
                        else
                        {
                            character.Attacks.Add(new Attack()
                            {
                                Name = spell.Definition.Name,
                                AttackBonus = character.ProficiencyBonus + spellcastingModifier,
                                Damage = GetUpcastDamageDie(damageModifier, spell.Definition.Level, i),
                                Level = i
                            });
                        }
                    }
                }
                else
                {
                    if (spell.Definition.RequiresSavingThrow)
                    {
                        var spellEffect = new SpellEffect()
                        {
                            Name = spell.Definition.Name,
                            SpellSaveAbilityId = spell.Definition.SaveDcAbilityId,
                            SpellSaveDc = GetSpellSaveDc(spellcastingModifier, character),
                            Damage = GetDamageDie(damageModifier, character)
                        };

                        character.SpellEffects.Add(spellEffect);
                    }
                    else
                    {
                        character.Attacks.Add(new Attack()
                        {
                            Name = spell.Definition.Name,
                            AttackBonus = character.ProficiencyBonus + spellcastingModifier,
                            Damage = GetDamageDie(damageModifier, character),
                        });
                    }
                }
            }
        }

        static int GetMaxSpellSlotLevel(Character character)
        {
            var maxSpellSlotLevel = 0;
            var maxPactMagicSlotLevel = 0;

            if (character.SpellSlots.Count > 0) maxSpellSlotLevel = character.SpellSlots.Max(s => s.Level);
            if (character.PactMagic.Count > 0) maxPactMagicSlotLevel = character.PactMagic.Max(s => s.Level);

            return Math.Max(maxSpellSlotLevel, maxPactMagicSlotLevel);
        }
    }

    private static Die GetUpcastDamageDie(SpellModifier damageModifier, int baseSlot, int currentSlot)
    {
        if (damageModifier.AtHigherLevels.HigherLevelDefinitions.Length != 1) throw new InvalidOperationException("Not an upcastable spell effect");

        var damageDie = Die.Zero;
        if (damageModifier.Die.DiceValue.HasValue && damageModifier.Die.DiceCount.HasValue)
        {
            damageDie = new Die() 
            { 
                Count = damageModifier.Die.DiceCount.Value, 
                Sides = damageModifier.Die.DiceValue.Value,
                Modifier = damageModifier.Die.FixedValue ?? 0
            };
        }

        var bonusDamagePerSlotLevel = damageModifier.AtHigherLevels.HigherLevelDefinitions[0].Dice;
        if (currentSlot > baseSlot && bonusDamagePerSlotLevel != null && bonusDamagePerSlotLevel.DiceCount.HasValue && bonusDamagePerSlotLevel.DiceValue.HasValue)
        {
            var levelDifference = currentSlot - baseSlot;
            damageDie = damageDie with
            {
                Count = damageDie.Count + (bonusDamagePerSlotLevel.DiceCount.Value * levelDifference),
            };
        }

        return damageDie;
    }

    private static Die GetDamageDie(SpellModifier damageModifier, Character character)
    {
        var higherLevelDefinition = damageModifier.AtHigherLevels.HigherLevelDefinitions
            .Where(d => d.Level <= character.Level)
            .OrderByDescending(d => d.Level)
            .FirstOrDefault();

        if (higherLevelDefinition != null && higherLevelDefinition.Dice != null && higherLevelDefinition.Dice.DiceValue.HasValue && higherLevelDefinition.Dice.DiceCount.HasValue)
        {
            return new Die() 
            { 
                Count = higherLevelDefinition.Dice.DiceCount.Value, 
                Sides = higherLevelDefinition.Dice.DiceValue.Value,
                Modifier = higherLevelDefinition.Dice.FixedValue ?? 0
            };
        }

        if (damageModifier.Die.DiceValue.HasValue && damageModifier.Die.DiceCount.HasValue)
        {
            return new Die() 
            { 
                Count = damageModifier.Die.DiceCount.Value, 
                Sides = damageModifier.Die.DiceValue.Value,
                Modifier = damageModifier.Die.FixedValue ?? 0
            };
        }

        return Die.Zero;
    }

    private static int GetSpellSaveDc(int spellcastingModifier, Character character) => 8 + spellcastingModifier + character.ProficiencyBonus;

    private void ReadSpellSlots(Data data, Character character)
    {
        var spellcasterLevel = 0f;
        foreach (var characterClass in data.Classes)
        {
            if (!characterClass.Definition.SpellCastingAbilityId.HasValue) continue;
            if (characterClass.Definition.Name == "Warlock") continue;

            var thisClassLevel = (float)characterClass.Level / characterClass.Definition.SpellRules.MultiClassSpellSlotDivisor;
            spellcasterLevel += characterClass.Definition.SpellRules.MultiClassSpellSlotRounding switch
            {
                MultiClassSpellSlotRounding.RoundDown => (float)Math.Floor(thisClassLevel),
                MultiClassSpellSlotRounding.RoundUp => (float)Math.Ceiling(thisClassLevel),
                _ => 0
            };
        }

        var slotsAtLevel = SlotsAtLevel.ElementAtOrDefault((int)spellcasterLevel);
        if (slotsAtLevel != null)
        {
            for (int i = 0; i < slotsAtLevel.Length; i++)
            {
                character.SpellSlots.Add(new SpellSlot()
                {
                    Level = i + 1,
                    Used = 0,
                    Available = slotsAtLevel[i]
                });
            }
        }

        var warlockClass = data.Classes.FirstOrDefault(c => c.Definition.Name == "Warlock");
        var warlockSlotsAtLevel = warlockClass?.Definition.SpellRules.LevelSpellSlots.ElementAtOrDefault(warlockClass.Level) ?? [];
        for (int i = 0; i < warlockSlotsAtLevel.Length; i++)
        {
            var level = i + 1;
            var slotsCount = warlockSlotsAtLevel[i];
            if (slotsCount == 0) continue;

            character.PactMagic.Add(new SpellSlot()
            {
                Level = level,
                Available = slotsCount,
                Used = 0
            });
        }
    }

    private static readonly int[][] SlotsAtLevel = [
        [],
        [2],
        [3],
        [4, 2],
        [4, 3],
        [4, 3, 2],
        [4, 3, 3],
        [4, 3, 3, 1],
        [4, 3, 3, 2],
        [4, 3, 3, 3, 1],
        [4, 3, 3, 3, 2],
        [4, 3, 3, 3, 2, 1],
        [4, 3, 3, 3, 2, 1],
        [4, 3, 3, 3, 2, 1, 1],
        [4, 3, 3, 3, 2, 1, 1],
        [4, 3, 3, 3, 2, 1, 1, 1],
        [4, 3, 3, 3, 2, 1, 1, 1],
        [4, 3, 3, 3, 2, 1, 1, 1, 1],
        [4, 3, 3, 3, 3, 1, 1, 1, 1],
        [4, 3, 3, 3, 3, 2, 1, 1, 1],
        [4, 3, 3, 3, 3, 2, 2, 1, 1]
    ];

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
            if (!weapon.Definition.Damage.DiceCount.HasValue) continue; 
            if (!weapon.Definition.Damage.DiceValue.HasValue) continue;
            if (!weapon.Definition.CategoryId.HasValue) continue;

            var damageDie = new Die()
            {
                Count = weapon.Definition.Damage.DiceCount.Value,
                Sides = weapon.Definition.Damage.DiceValue.Value
            };

            var isFinesseWeapon = weapon.Definition.Properties.Any(p => p.Name == "Finesse");
            var attackBonus = isFinesseWeapon switch
            {
                true => Math.Max(character.StrengthModifier, character.DexterityModifier),
                false => character.StrengthModifier
            };

            var weaponCategory = weapon.Definition.CategoryId.Value switch
            {
                WeaponCategories.Simple => "simple-weapons",
                WeaponCategories.Martial => "martial-weapons",
                _ => ""
            };
            var weaponCategoryProficiency = GetProficiencyBonus(data.Modifiers, weaponCategory, character);
            var weaponProficiency = GetProficiencyBonus(data.Modifiers, weapon.Definition.Name.ToLower(), character);

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

    private static void ReadAc(Data data, Character character)
    {
        var equippedArmor = data.Inventory
            .Where(e => e.Equipped)
            .Where(e => e.Definition.ArmorTypeId.HasValue && e.Definition.ArmorTypeId != ArmorTypes.Shield)
            .Where(e => !e.Definition.CanAttune || e.IsAttuned)
            .FirstOrDefault();

        var equippedShield = data.Inventory
            .Where(e => e.Equipped)
            .Where(e => e.Definition.ArmorTypeId == ArmorTypes.Shield)
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
            if (equippedArmor?.Definition.ArmorTypeId != ArmorTypes.Heavy) armorClass += Math.Min(character.DexterityModifier, 2);
            if (equippedShield != null) armorClass += equippedShield.Definition.ArmorClass ?? 0;

            character.ArmorClass = armorClass;
        }
    }

    private static void ReadBaseDetails(Data data, Character character)
    {
        character.Name = data.Name;
        character.Level = data.Classes.Sum(c => c.Level);
        character.HitDice = [.. data.Classes.Select(c => new Die() { Count = c.Level, Sides = c.Definition.HitDice })];
        character.ProficiencyBonus = 1 + (int)Math.Ceiling(character.Level / 4f);

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

        character.StrengthSavingThrowModifier = character.StrengthModifier + GetProficiencyBonus(data.Modifiers, "strength-saving-throws", character);
        character.DexteritySavingThrowModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "dexterity-saving-throws", character);
        character.ConstitutionSavingThrowModifier = character.ConstitutionModifier + GetProficiencyBonus(data.Modifiers, "constitution-saving-throws", character);
        character.IntelligenceSavingThrowModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "intelligence-saving-throws", character);
        character.WisdomSavingThrowModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "wisdom-saving-throws", character);
        character.CharismaSavingThrowModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "charisma-saving-throws", character);

        character.AcrobaticsModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "acrobatics", character);
        character.AnimalHandlingModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "animal-handling", character);
        character.ArcanaModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "arcana", character);
        character.AthleticsModifier = character.StrengthModifier + GetProficiencyBonus(data.Modifiers, "athletics", character);
        character.DeceptionModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "deception", character);
        character.HistoryModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "history", character);
        character.InsightModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "insight", character);
        character.IntimidationModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "intimidation", character);
        character.InvestigationModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "investigation", character);
        character.MedicineModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "medicine", character);
        character.NatureModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "nature", character);
        character.PerceptionModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "perception", character);
        character.PerformanceModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "performance", character);
        character.PersuasionModifier = character.CharismaModifier + GetProficiencyBonus(data.Modifiers, "persuasion", character);
        character.ReligionModifier = character.IntelligenceModifier + GetProficiencyBonus(data.Modifiers, "religion", character);
        character.SleightOfHandModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "sleight-of-hand", character);
        character.StealthModifier = character.DexterityModifier + GetProficiencyBonus(data.Modifiers, "stealth", character);
        character.SurvivalModifier = character.WisdomModifier + GetProficiencyBonus(data.Modifiers, "survival", character);

        character.PassivePerception = 10 + character.PerceptionModifier;
        character.MaxHp = CalculateMaxHp(data, character);
        character.CurrentHp = character.MaxHp - data.RemovedHitPoints;
        character.TemporaryHp = data.TemporaryHitPoints;
    }

    private static int CalculateMaxHp(Data data, Character character)
    {
        var classes = data.Classes;
        var totalHp = 0;

        foreach (var characterClass in classes)
        {
            int level = characterClass.Level;
            int hitDie = characterClass.Definition.HitDice;
            int fixedPerLevel = (hitDie / 2) + 1;

            if (characterClass.IsStartingClass)
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

            var activeClassFeatures = characterClass.ClassFeatures.Where(f => f.Definition.RequiredLevel <= level).Select(f => f.Definition.Id).ToArray();
            foreach (var modifier in data.Modifiers.Class)
            {
                if (!activeClassFeatures.Contains(modifier.ComponentId)) continue;
                if (modifier.Type != Types.Bonus || modifier.SubType != "hit-points-per-level") continue;

                totalHp += (modifier.FixedValue ?? 0) * level;
            }
        }

        // Racial modifiers are always active
        var raceModifiers = data.Modifiers.Race.Where(m => m.Type == Types.Bonus && m.SubType == "hit-points-per-level");
        foreach (var modifier in raceModifiers)
        {
            totalHp += (modifier.FixedValue ?? 0) * character.Level;
        }

        return totalHp;
    }

    private static int GetProficiencyBonus(ModifiersTable modifiers, string subType, Character character)
    {
        var isProficient = modifiers.Race.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Class.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Background.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Item.Any(m => m.Type == Types.Proficiency && m.SubType == subType)
            || modifiers.Feat.Any(m => m.Type == Types.Proficiency && m.SubType == subType);

        if (!isProficient) return 0;

        return character.ProficiencyBonus;
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
