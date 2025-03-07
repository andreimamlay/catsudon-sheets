using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CatsUdon.CharacterSheets.Adapters.Abstractions;
using CatsUdon.CharacterSheets.CCFolia;
using System.Text;
using System.Text.RegularExpressions;

namespace CatsUdon.CharacterSheets.Adapters.DndJp;

public partial class DndJpAdapter(HttpClient httpClient) : ICharacterSheetAdapter
{
    [GeneratedRegex(@"^https\:\/\/dndjp.sakura.ne.jp\/OUTPUT.php\?ID=(\d+)$")]
    private static partial Regex UrlMatchRegex { get; }

    [GeneratedRegex(@"^(?<attackName>.*)\s*(?<attackBonus>[\+\-]?(\d+))\s*(?<damage>\d+d\d+[\+\-]?\d*)$")]
    private static partial Regex NameBonusDamageExtraAttackRegex { get; }

    [GeneratedRegex(@"^(?<attackName>.*)\s*(?<damage>\d+d\d+[\+\-]?\d*)$")]
    private static partial Regex NameDamageExtraAttackRegex { get; }

    public bool CanConvert(string url) => UrlMatchRegex.IsMatch(url);

    public async Task<CharacterSheet> Convert(string url)
    {
        if (!CanConvert(url))
        {
            throw new ArgumentException("URL is not supported", nameof(url));
        }

        var getHtmlResponse = await httpClient.GetAsync(url);
        getHtmlResponse.EnsureSuccessStatusCode();

        var parser = new HtmlParser();
        var document = parser.ParseDocument(await getHtmlResponse.Content.ReadAsStreamAsync());

        var character = new Character();

        ReadStatus(character, document);
        ReadAbilitiesAndSkills(character, document);
        ReadAttacks(character, document);
        ReadSpellSlots(character, document);

        return new CharacterSheet()
        {
            CCFoliaCharacter = ConvertToCCFoliaCharacter(character)
        };
    }

    private static CCFoliaCharacter ConvertToCCFoliaCharacter(Character character)
    {
        var ccfoliaCharacter = new CCFoliaCharacter();
        var data = ccfoliaCharacter.Data;
        data.Name = character.Name;
        data.Initiative = 0;

        data.Status.Add(new CCFoliaStatus() { Label = "HP", Value = character.CurrentHp, Max = character.MaxHp });
        data.Status.Add(new CCFoliaStatus() { Label = "ー時HP", Value = character.TemporaryHp });
        data.Status.Add(new CCFoliaStatus() { Label = "AC", Value = character.ArmorClass });
        data.Status.Add(new CCFoliaStatus() { Label = "インスピ", Value = character.Inspiration });

        foreach (var (level, (used, total)) in character.SpellSlots)
        {
            data.Status.Add(new CCFoliaStatus()
            {
                Label = $"スロット{level}",
                Value = total - used,
                Max = total
            });
        }

        foreach (var (ability, score) in character.AbilityScores)
        {
            data.Params.Add(new CCFoliaParameter() { Label = Translate.ToJapaneese(ability), Value = score.Value.ToString() });
        }

        data.Params.Add(new CCFoliaParameter() { Label = "受動知覚", Value = character.PassivePerception.ToString() });

        var commands = new StringBuilder();
        commands.AppendLine($"1d20{character.Initiative} イニシアチブ");
        if (character.HitDice.HasValue)
        {
            commands.AppendLine($"1d{character.HitDice.Value.Sides} ヒットダイスでHP回復");
        }

        if (character.Attacks.Count > 0) 
        {
            commands.AppendLine("=================  攻撃  ================");
            foreach (var attack in character.Attacks)
            {
                if (attack.AttackBonus.HasValue)
                {
                    commands.AppendLine($"1d20{attack.AttackBonus} 【{attack.Name}】 攻撃ロール");
                }

                commands.AppendLine($"{attack.Damage} 【{attack.Name}】 ダメージ");

                if (attack.AttackBonus.HasValue)
                {
                    var criticalDie = attack.Damage with
                    {
                        Count = attack.Damage.Count * 2
                    };

                    commands.AppendLine($"{criticalDie} 【{attack.Name}】 クリティカル");
                }
            }
        }

        if (character.SavingThrows.Count > 0)
        {
            commands.AppendLine("===========  セーヴィングスロー  ==========");
            foreach (var (ability, modifier) in character.SavingThrows)
            {
                commands.AppendLine($"1d20{modifier} 【{Translate.ToJapaneese(ability)}】セーヴィングスロー");
            }
        }

        if (character.AbilityScores.Count > 0)
        {
            commands.AppendLine("=============  能力値判定  ===============");
            foreach (var (ability, modifier) in character.AbilityScores)
            {
                commands.AppendLine($"1d20{modifier} 【{Translate.ToJapaneese(ability)}】能力値判定");
            }
        }

        if (character.Skills.Count > 0)
        {
            commands.AppendLine("=============  技能判定  ================");
            foreach (var (skill, die) in character.Skills)
            {
                commands.AppendLine($"{die} 【{Translate.ToJapaneese(skill)}】技能判定");
            }
        }

        ccfoliaCharacter.Data.Commands = commands.Replace("\r\n", "\n").ToString();

        return ccfoliaCharacter;
    }

    private static void ReadStatus(Character character, IHtmlDocument document)
    {
        character.Name = document.QuerySelectorText(CssSelectors.CharacterName);

        var initiativeBonus = document.QuerySelectorNumber(CssSelectors.Initiative);
        character.Initiative = initiativeBonus;

        character.MaxHp = document.QuerySelectorNumber(CssSelectors.MaxHp);
        character.CurrentHp = document.QuerySelectorNumber(CssSelectors.CurrentHp);
        character.TemporaryHp = document.QuerySelectorNumber(CssSelectors.TempHp);
        character.ArmorClass = document.QuerySelectorNumber(CssSelectors.ArmorClass);
        character.Inspiration = document.QuerySelectorNumber(CssSelectors.Inspiration);

        var hitDiceString = document.QuerySelectorText(CssSelectors.HitDice).Trim();
        if (Die.TryParse(hitDiceString, out var die))
        {
            character.HitDice = die.Value;
        }
    }


    private static void ReadAbilitiesAndSkills(Character character, IHtmlDocument document)
    {
        character.PassivePerception = document.QuerySelectorNumber(CssSelectors.PassivePerception);

        foreach (var (ability, selector) in CssSelectors.AbilityScores)
        {
            character.AbilityScores.Add((ability, document.QuerySelectorNumber(selector)));
        }

        foreach (var (ability, selector) in CssSelectors.SavingThrows)
        {
            character.SavingThrows.Add((ability, document.QuerySelectorNumber(selector)));
        }

        foreach (var (skill, selector) in CssSelectors.Skills)
        {
            var die = new Die()
            {
                Count = 1,
                Sides = 20,
                Modifier = document.QuerySelectorNumber(selector)
            };

            character.Skills.Add((skill, die));
        }
    }

    private static void ReadAttacks(Character character, IHtmlDocument document)
    {
        for (var i = 0; i < CssSelectors.Attacks.Length; i++)
        {
            var row = document.QuerySelector(CssSelectors.Attacks[i]);
            if (row == null) continue;

            var attackName = row.QuerySelectorText("td:nth-child(1)");
            if (string.IsNullOrWhiteSpace(attackName)) continue;

            var attackBonus = row.QuerySelectorNumberOptional("td:nth-child(2)");
            var damage = row.QuerySelectorText("td:nth-child(3)");
            if (!Die.TryParse(damage, out var die)) continue;

            character.Attacks.Add(new Attack()
            {
                Name = attackName,
                AttackBonus = attackBonus,
                Damage = die.Value
            });
        }

        var extraAttaks = document.QuerySelectorText(CssSelectors.AttacksExtra).Trim().Split("\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var extraAttack in extraAttaks)
        {
            var match = NameBonusDamageExtraAttackRegex.Match(extraAttack);
            if (match.Success)
            {
                var attackName = match.Groups["attackName"].Value;
                var attackBonus = int.Parse(match.Groups["attackBonus"].Value);
                var damage = match.Groups["damage"].Value;

                if (Die.TryParse(damage, out var die))
                {
                    character.Attacks.Add(new Attack()
                    {
                        Name = attackName,
                        AttackBonus = attackBonus,
                        Damage = die.Value
                    });
                }

                continue;
            }

            match = NameDamageExtraAttackRegex.Match(extraAttack);
            if (match.Success)
            {
                var attackName = match.Groups["attackName"].Value;
                var damage = match.Groups["damage"].Value;
                if (Die.TryParse(damage, out var die))
                {
                    character.Attacks.Add(new Attack()
                    {
                        Name = attackName,
                        Damage = die.Value
                    });
                }
                continue;
            }
        }
    }

    private void ReadSpellSlots(Character character, IHtmlDocument document)
    {
        for (var i = 0; i < CssSelectors.Slots.Length; i++)
        {
            var (totalSlots, usedSlots) = CssSelectors.Slots[i];
            var total = document.QuerySelectorNumberOptional(totalSlots);
            if (!total.HasValue) continue;

            var used = document.QuerySelectorNumber(usedSlots);

            character.SpellSlots[i + 1] = (used, total.Value);
        }
    }
}
