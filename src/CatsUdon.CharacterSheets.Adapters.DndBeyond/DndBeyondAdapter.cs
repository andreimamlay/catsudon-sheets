using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.Adapters.DndBeyond.Models;
using CatsUdon.CharacterSheets.CCFolia;
using System.Text;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.DndBeyond;

internal partial class DndBeyondAdapter(IDndBeyondApiClient apiClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https:\/\/www\.dndbeyond\.com\/characters\/(?<characterId>\d+)(\/.*)?$")]
    private static partial Regex UrlMatchRegex { get; }

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

        var characterData = await apiClient.GetCharacterAsync(characterId);

        var character = new Character();
        ReadBaseDetails(characterData, character);
        ReadAc(characterData, character);
        ReadWeaponAttacks(characterData, character);
        ReadActions(characterData, character);
        ReadSpellSlots(characterData, character);
        await ReadSpellEffects(characterData, character);

        return new CharacterSheet()
        {
            Character = ConvertToCCFoliaCharacter(character)
        };
    }

    private static CCFoliaCharacterClipboardData ConvertToCCFoliaCharacter(Character character)
    {
        var ccfoliaCharacter = new CCFoliaCharacterClipboardData();
        var data = ccfoliaCharacter.Data;
        data.Name = character.Name;
        data.Initiative = 0;

        data.Status.Add(new CCFoliaStatus() { Label = "HP", Value = character.CurrentHp, Max = character.MaxHp });
        data.Status.Add(new CCFoliaStatus() { Label = "Temp HP", Value = character.TemporaryHp });
        data.Status.Add(new CCFoliaStatus() { Label = "AC", Value = character.ArmorClass });
        data.Status.Add(new CCFoliaStatus() { Label = "Inspiration", Value = character.Inspiration });

        foreach (var spellSlot in character.SpellSlots)
        {
            data.Status.Add(new CCFoliaStatus()
            {
                Label = $"Slot {spellSlot.Level}",
                Value = spellSlot.Available,
                Max = spellSlot.Available
            });
        }

        foreach (var pactSlot in character.PactMagic)
        {
            data.Status.Add(new CCFoliaStatus()
            {
                Label = $"Pact {pactSlot.Level}",
                Value = pactSlot.Available,
                Max = pactSlot.Available
            });
        }

        data.Params.Add(new CCFoliaParameter() { Label = "STR", Value = ToModifierString(character.StrengthModifier) });
        data.Params.Add(new CCFoliaParameter() { Label = "DEX", Value = ToModifierString(character.DexterityModifier) });
        data.Params.Add(new CCFoliaParameter() { Label = "CON", Value = ToModifierString(character.ConstitutionModifier) });
        data.Params.Add(new CCFoliaParameter() { Label = "INT", Value = ToModifierString(character.IntelligenceModifier) });
        data.Params.Add(new CCFoliaParameter() { Label = "WIS", Value = ToModifierString(character.WisdomModifier) });
        data.Params.Add(new CCFoliaParameter() { Label = "CHA", Value = ToModifierString(character.CharismaModifier) });

        var memoBuilder = new StringBuilder();
        memoBuilder.AppendLine($"Passive Perception {character.PassivePerception}");
        memoBuilder.AppendLine($"Passive Investigation {character.PassiveInvestigation}");
        memoBuilder.AppendLine($"Passive Insight {character.PassiveInsight}");

        data.Memo = memoBuilder.Replace("\r\n", "\n").ToString().Trim();

        var commands = new StringBuilder();
        commands.AppendLine($"1d20{ToDiceModifierString(character.DexterityModifier)} Initiative");
        foreach (var hitDie in character.HitDice)
        {
            commands.AppendLine($"1d{hitDie.Sides} Hit dice (max {hitDie.Count} times)");
        }

        if (character.Attacks.Count > 0)
        {
            commands.AppendLine("=================  Attacks  ================");
            foreach (var attackGroup in character.Attacks.GroupBy(a => a.Name))
            {
                foreach (var (index, attack) in attackGroup.Index())
                {
                    if (index == 0 && !attack.HideAttack) commands.AppendLine($"1d20{attack.AttackBonus} [{attack.Name}] Attack roll");

                    if (attack.Level.HasValue)
                    {
                        commands.AppendLine($"{attack.Damage} [{attack.Name}] [Slot {attack.Level}] {Tags(attack.Tags)}Damage");
                        if (!attack.CanNotCrit)
                        {
                            var criticalDie = attack.Damage with
                            {
                                Count = attack.Damage.Count * 2
                            };
                            commands.AppendLine($"{criticalDie} [{attack.Name}] [Slot {attack.Level}] {Tags(attack.Tags)}Critical");
                        }
                    }
                    else
                    {
                        commands.AppendLine($"{attack.Damage} [{attack.Name}] {Tags(attack.Tags)}Damage");
                        if (!attack.CanNotCrit)
                        {
                            var criticalDie = attack.Damage with
                            {
                                Count = attack.Damage.Count * 2
                            };
                            commands.AppendLine($"{criticalDie} [{attack.Name}] {Tags(attack.Tags)}Critical");
                        }
                    }
                }
            }


        }

        if (character.SpellEffects.Count > 0)
        {
            commands.AppendLine("=================  Spells  ================");
            foreach (var spellEffect in character.SpellEffects)
            {
                if (spellEffect.Level.HasValue)
                {
                    commands.AppendLine($"{spellEffect.Damage} [{spellEffect.Name}] [Slot {spellEffect.Level}] Damage");
                }
                else
                {
                    commands.AppendLine($"{spellEffect.Damage} [{spellEffect.Name}] Damage");
                }
            }
        }

        commands.AppendLine("===========  Saving Throws  ==========");
        commands.AppendLine($"1d20{ToDiceModifierString(character.StrengthSavingThrowModifier)} [Strength] Saving throw");
        commands.AppendLine($"1d20{ToDiceModifierString(character.DexteritySavingThrowModifier)} [Dexterity] Saving throw");
        commands.AppendLine($"1d20{ToDiceModifierString(character.ConstitutionSavingThrowModifier)} [Constitution] Saving throw");
        commands.AppendLine($"1d20{ToDiceModifierString(character.IntelligenceSavingThrowModifier)} [Intelligence] Saving throw");
        commands.AppendLine($"1d20{ToDiceModifierString(character.WisdomSavingThrowModifier)} [Wisdom] Saving throw");
        commands.AppendLine($"1d20{ToDiceModifierString(character.CharismaSavingThrowModifier)} [Charisma] Saving throw");

        commands.AppendLine("=============  Abilities  ===============");
        commands.AppendLine($"1d20{ToDiceModifierString(character.AcrobaticsModifier)} [Acrobatics] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.AnimalHandlingModifier)} [Animal Handling] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.ArcanaModifier)} [Arcana] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.AthleticsModifier)} [Athletics] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.DeceptionModifier)} [Deception] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.HistoryModifier)} [History] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.InsightModifier)} [Insight] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.IntimidationModifier)} [Intimidation] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.InvestigationModifier)} [Investigation] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.MedicineModifier)} [Medicine] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.NatureModifier)} [Nature] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.PerceptionModifier)} [Perception] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.PerformanceModifier)} [Performance] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.PersuasionModifier)} [Persuasion] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.ReligionModifier)} [Religion] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.SleightOfHandModifier)} [Sleight Of Hand] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.StealthModifier)} [Stealth] Ability check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.SurvivalModifier)} [Survival] Ability check");

        commands.AppendLine("=============  Skills  ================");
        commands.AppendLine($"1d20{ToDiceModifierString(character.StrengthModifier)} [Strength] Skill check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.DexterityModifier)} [Dexterity] Skill check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.ConstitutionModifier)} [Constitution] Skill check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.IntelligenceModifier)} [Intelligence] Skill check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.WisdomModifier)} [Wisdom] Skill check");
        commands.AppendLine($"1d20{ToDiceModifierString(character.CharismaModifier)} [Charisma] Skill check");

        ccfoliaCharacter.Data.Commands = commands.Replace("\r\n", "\n").ToString().Trim();

        return ccfoliaCharacter;

        string ToModifierString(int value) => value switch
        {
            > 0 => $"+{value}",
            0 => "0",
            < 0 => $"{value}"
        };
        string ToDiceModifierString(int value) => value switch
        {
            > 0 => $"+{value}",
            0 => "",
            < 0 => $"{value}"
        };
        string Tags(string[] tags)
        {
            if (tags.Length == 0) return string.Empty;

            return $"[{String.Join(", ", tags)}] ";
        }
    }

    private async Task ReadSpellEffects(CharacterData data, Character character)
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

            var spells = classSpells.Spells.Select(s => s.Definition);

            if (characterClass.SubclassDefinition != null)
            {
                var alwaysPreparedSpells = await apiClient.GetAlwaysPreparedSpellsAsync(characterClass.SubclassDefinition.Id, characterClass.Level);
                spells = spells.Concat(alwaysPreparedSpells.Select(s => s.Definition));
            }

            foreach (var spell in spells.DistinctBy(d => d.Id))
            {
                ReadSpell(spell, character, maxSlotLevel, spellcastingModifier);
            }
        }

        ReadSpells(data.Spells.Race, character, maxSlotLevel);
        ReadSpells(data.Spells.Class, character, maxSlotLevel);
        ReadSpells(data.Spells.Background, character, maxSlotLevel);
        ReadSpells(data.Spells.Item, character, maxSlotLevel);
        ReadSpells(data.Spells.Feat, character, maxSlotLevel);

        void ReadSpells(Spell[]? spells, Character character, int maxSlotLevel)
        {
            if (spells == null) return;

            foreach (var spell in spells)
            {
                if (spell.DisplayAsAttack.HasValue && !spell.DisplayAsAttack.Value) continue;

                var spellcastingModifier = spell.SpellCastingAbilityId switch
                {
                    StatIds.Strength => character.StrengthModifier,
                    StatIds.Dexterity => character.DexterityModifier,
                    StatIds.Constitution => character.ConstitutionModifier,
                    StatIds.Intelligence => character.IntelligenceModifier,
                    StatIds.Wisdom => character.WisdomModifier,
                    StatIds.Charisma => character.CharismaModifier,
                    _ => 0
                };

                ReadSpell(spell.Definition, character, maxSlotLevel, spellcastingModifier);
            }
        }

        void ReadSpell(SpellDefinition spell, Character character, int maxSlotLevel, int spellcastingModifier)
        {
            var damageModifier = spell.Modifiers.FirstOrDefault(m => m.Type == "damage");
            if (damageModifier == null) return;

            if (damageModifier.AtHigherLevels.HigherLevelDefinitions.Length == 1)
            {
                // Upcastable spell with scaling
                for (int i = spell.Level; i <= maxSlotLevel; i++)
                {
                    if (spell.RequiresSavingThrow)
                    {
                        var spellEffect = new SpellEffect()
                        {
                            Name = spell.Name,
                            SpellSaveAbilityId = spell.SaveDcAbilityId,
                            SpellSaveDc = GetSpellSaveDc(spellcastingModifier, character),
                            Damage = GetUpcastDamageDie(damageModifier, spell.Level, i),
                            Level = i
                        };

                        character.SpellEffects.Add(spellEffect);
                    }
                    else
                    {
                        character.Attacks.Add(new Attack()
                        {
                            Name = spell.Name,
                            AttackBonus = character.ProficiencyBonus + spellcastingModifier,
                            Damage = GetUpcastDamageDie(damageModifier, spell.Level, i),
                            Level = i
                        });
                    }
                }
            }
            else
            {
                if (spell.RequiresSavingThrow)
                {
                    var spellEffect = new SpellEffect()
                    {
                        Name = spell.Name,
                        SpellSaveAbilityId = spell.SaveDcAbilityId,
                        SpellSaveDc = GetSpellSaveDc(spellcastingModifier, character),
                        Damage = GetDamageDie(damageModifier, character)
                    };

                    character.SpellEffects.Add(spellEffect);
                }
                else if (spell.RequiresAttackRoll)
                {
                    if (spell.AttackType == AttackType.Melee)
                    {
                        // This spell is attached to melee weapon attack
                        character.Attacks.Add(new Attack()
                        {
                            Name = spell.Name,
                            AttackBonus = 0,
                            HideAttack = true,
                            CanNotCrit = true,
                            Damage = GetDamageDie(damageModifier, character),
                        });
                    }
                    else
                    {
                        character.Attacks.Add(new Attack()
                        {
                            Name = spell.Name,
                            AttackBonus = character.ProficiencyBonus + spellcastingModifier,
                            Damage = GetDamageDie(damageModifier, character),
                        });
                    }
                }
                else
                {
                    character.Attacks.Add(new Attack()
                    {
                        Name = spell.Name,
                        AttackBonus = 0,
                        HideAttack = true,
                        CanNotCrit = true,
                        Damage = GetDamageDie(damageModifier, character),
                    });
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
            if (damageModifier.AtHigherLevels.HigherLevelDefinitions.Length > 1)
            {
                // Use value from the definition as it seems to be precomputed
                return new Die()
                {
                    Count = higherLevelDefinition.Dice.DiceCount.Value,
                    Sides = higherLevelDefinition.Dice.DiceValue.Value,
                    Modifier = higherLevelDefinition.Dice.FixedValue ?? 0
                };
            }
            else
            {
                // Treat value from definition as a bonus value
                if (damageModifier.Die.DiceCount.HasValue)
                {
                    return new Die()
                    {
                        Count = damageModifier.Die.DiceCount.Value + higherLevelDefinition.Dice.DiceCount.Value,
                        Sides = higherLevelDefinition.Dice.DiceValue.Value,
                        Modifier = higherLevelDefinition.Dice.FixedValue ?? 0
                    };
                }
            }
        }
        else
        {
            // No scaling, use base damage only
            if (damageModifier.Die.DiceValue.HasValue && damageModifier.Die.DiceCount.HasValue)
            {
                return new Die()
                {
                    Count = damageModifier.Die.DiceCount.Value,
                    Sides = damageModifier.Die.DiceValue.Value,
                    Modifier = damageModifier.Die.FixedValue ?? 0
                };
            }
        }

        return Die.Zero;
    }

    private static int GetSpellSaveDc(int spellcastingModifier, Character character) => 8 + spellcastingModifier + character.ProficiencyBonus;

    private void ReadSpellSlots(CharacterData data, Character character)
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

    private static void ReadWeaponAttacks(CharacterData data, Character character)
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



            var isFinesseWeapon = weapon.Definition.Properties.Any(p => p.Name == "Finesse");
            var attackBonus = isFinesseWeapon switch
            {
                true => Math.Max(character.StrengthModifier, character.DexterityModifier),
                false => character.StrengthModifier
            };
            var damageBonus = isFinesseWeapon switch
            {
                true => Math.Max(character.StrengthModifier, character.DexterityModifier),
                false => character.StrengthModifier
            };

            var damageDie = new Die()
            {
                Count = weapon.Definition.Damage.DiceCount.Value,
                Sides = weapon.Definition.Damage.DiceValue.Value,
                Modifier = damageBonus
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
                        Tags = ["2H"],
                        AttackBonus = new Modifier(attackBonus),
                        Damage = versatileDamageDie.Value with
                        {
                            Modifier = damageBonus
                        }
                    });
                }
            }
        }
    }

    private void ReadActions(CharacterData characterData, Character character)
    {
        ReadAction(characterData.Actions.Race, character);
        ReadAction(characterData.Actions.Class, character);
        ReadAction(characterData.Actions.Background, character);
        ReadAction(characterData.Actions.Item, character);
        ReadAction(characterData.Actions.Feat, character);

        static void ReadAction(CharacterAction[]? actions, Character character)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                if (action.Dice == null) continue;
                if (!action.Dice.DiceCount.HasValue) continue;
                if (!action.Dice.DiceValue.HasValue) continue;
                if (action.DisplayAsAttack.HasValue && !action.DisplayAsAttack.Value) continue;

                var damageDie = new Die()
                {
                    Count = action.Dice.DiceCount.Value,
                    Sides = action.Dice.DiceValue.Value,
                    Modifier = action.Dice.FixedValue ?? 0
                };

                if (action.SaveStatId.HasValue)
                {
                    character.SpellEffects.Add(new SpellEffect()
                    {
                        Name = action.Name,
                        SpellSaveAbilityId = action.SaveStatId,
                        Damage = damageDie
                    });
                }
                else
                {
                    character.Attacks.Add(new Attack()
                    {
                        Name = action.Name,
                        Damage = damageDie
                    });
                }
            }
        }
    }

    private static void ReadAc(CharacterData data, Character character)
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

    private static void ReadBaseDetails(CharacterData data, Character character)
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
        character.PassiveInvestigation = 10 + character.InvestigationModifier;
        character.PassiveInsight = 10 + character.InsightModifier;
        character.MaxHp = CalculateMaxHp(data, character);
        character.CurrentHp = character.MaxHp - data.RemovedHitPoints;
        character.TemporaryHp = data.TemporaryHitPoints;
    }

    private static int CalculateMaxHp(CharacterData data, Character character)
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

    private static int SumAbilityScores(StatIds statId, CharacterData data)
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
